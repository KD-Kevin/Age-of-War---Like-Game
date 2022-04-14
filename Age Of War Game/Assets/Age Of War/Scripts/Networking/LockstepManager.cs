using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

[System.Runtime.InteropServices.Guid("3749176F-32B4-4DB0-9BA8-F4493743DA0B")]
public class LockstepManager : MonoBehaviour
{
    public ActionTurn CurrentTurn { get; set; }
    public PlayerActions LocalPlayersCurrentTurn { get; set; }
    public ActionTurn PendingTurn { get; set; }
    public ActionTurn ConfirmedTurn { get; set; }
    public ActionTurn ProcessingTurn { get; set; }
    public TurnHistory ActionTurnHistory = new TurnHistory();


    public static LockstepManager Instance = null;
    private int NumberOfGameTurnsPerLockstepTurn = 4; // Adjusts based on the connection ei: average time per round trip = 167, game now does 5 game turns per lockstep (Math.Ciel(167/40) = 5)
    private int NumberOfFixedUpdatesPerGameTurn = 2;
    private int GameTurnCounter = 0;
    private float WaitTime = 0;
    private bool ReconnectOnNextGameTurn = false;
    private bool CountDown = false;

    public int SecondsTillReconnect { get; set; }
    public int ReconnectOnSecond { get; set; }

    public int FixedFrameCounter { get; private set; }
    public int FixedGameTurnCounter { get; private set; }
    public int LockstepTurnCounter { get; private set; }

    public bool Reconnecting { get; set; }

    public bool SimulationStarted { get; set; }

    public bool SimulationPaused { get; set; }

    public bool WaitingOnPlayer { get; set; }

    private void Awake()
    {
        Instance = this;
        FixedFrameCounter = 0;
        FixedGameTurnCounter = 0;
        LockstepTurnCounter = 0;
    }

    private void Update()
    {
        if (WaitingOnPlayer)
        {
            WaitTime += Time.deltaTime;

            if (PendingTurn != null && ConfirmedTurn != null)
            {
                WaitingOnPlayer = !(PendingTurn.ReadyForNextTurn() && ConfirmedTurn.ReadyForNextTurn());
            }
            // Turn 2
            else if (PendingTurn != null)
            {
                WaitingOnPlayer = !PendingTurn.ReadyForNextTurn();
            }
            Reconnecting = !WaitingOnPlayer;
        }
        else if (Reconnecting)
        {
            // Now that everyone has the latest lockstep, Make sure everyone reconnects at the same time
            if (WaitTime < Time.fixedDeltaTime * NumberOfFixedUpdatesPerGameTurn * 0.5f)
            {
                // Process Turn and reconnect - Everyone should have gotten the confirmation around the same time, but may have gone over the (WaitTime < Time.fixedDeltaTime)
                ReconnectOnNextGameTurn = true;
            }
            else if (WaitTime < Time.fixedDeltaTime * NumberOfFixedUpdatesPerGameTurn * NumberOfGameTurnsPerLockstepTurn)
            {
                // Small Disconnect
                CountDown = true;
                ReconnectOnNextGameTurn = true;
                ReconnectOnSecond = System.DateTime.Now.Second + 2;
                SecondsTillReconnect = 2;

                if (ReconnectOnSecond > 60)
                {
                    ReconnectOnSecond -= 60;
                }
            }
            else
            {
                // Large Disconnect
                CountDown = true;
                ReconnectOnNextGameTurn = true;
                ReconnectOnSecond = System.DateTime.Now.Second + 10;
                SecondsTillReconnect = 10;

                if (ReconnectOnSecond > 60)
                {
                    ReconnectOnSecond -= 60;
                }
            }
            WaitTime = 0;
            Reconnecting = false;
        }
        else if (!SimulationStarted)
        {
            if (PlayerManager.Instance.EveryoneIsReadyForStart())
            {
                CountDown = true;
                ReconnectOnSecond = System.DateTime.Now.Second + 6;
                SecondsTillReconnect = 6;

                if (ReconnectOnSecond > 60)
                {
                    ReconnectOnSecond -= 60;
                }

                SimulationStarted = true;

                LocalPlayersCurrentTurn = new PlayerActions(PlayerManager.Instance.LocalPlayer.PlayerID, true);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!SimulationStarted)
        {
            return;
        }

        if (SimulationPaused)
        {
            return;
        }

        if (WaitingOnPlayer)
        {
            return;
        }

        if (Reconnecting)
        {
            return;
        }

        if (CountDown)
        {
            if (System.DateTime.Now.Second == ReconnectOnSecond)
            {
                CountDown = false;
                SecondsTillReconnect = 0;
            }
            else
            {
                int CurrentSecond = System.DateTime.Now.Second;
                if (CurrentSecond > ReconnectOnSecond)
                {
                    SecondsTillReconnect = 60 + ReconnectOnSecond - CurrentSecond;
                }
                else
                {
                    SecondsTillReconnect = ReconnectOnSecond - CurrentSecond;
                }
                return;
            }
        }

        //Debug.Log($"Fixed Delta Times ({Time.fixedDeltaTime})");
        FixedFrameCounter++;
        if (FixedFrameCounter == NumberOfFixedUpdatesPerGameTurn)
        {
            FixedFrameCounter = 0;
            GameTurn();
        }
        else
        {
            AsyncGameTurn();
        }
    }

    public void AsyncGameTurn()
    {
        Debug.Log($"Async Update {System.DateTime.Now.Hour} hr / {System.DateTime.Now.Minute} min / {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
    }

    public void GameTurn()
    {
        GameTurnCounter++;
        if (ReconnectOnNextGameTurn)
        {
            ReconnectedLockstepTurn();
        }

        FixedGameTurnCounter++;
        if (FixedGameTurnCounter == NumberOfGameTurnsPerLockstepTurn)
        {
            FixedGameTurnCounter = 0;
            LockstepTurn();
        }

        Debug.Log($"Game Update {System.DateTime.Now.Hour} hr / {System.DateTime.Now.Minute} min / {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
    }

    public void LockstepTurn()
    {
        // Turn 3 and above
        if (PendingTurn != null && ConfirmedTurn != null)
        {
            WaitingOnPlayer = !(PendingTurn.ReadyForNextTurn() && ConfirmedTurn.ReadyForNextTurn());
            if (!WaitingOnPlayer)
            {
                LockstepTurnCounter++;
                ConfirmedTurn?.NextTurn();
                PendingTurn?.NextTurn();
                CurrentTurn?.NextTurn();
                CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            }
        }
        // Turn 2
        else if (PendingTurn != null)
        {
            WaitingOnPlayer = !PendingTurn.ReadyForNextTurn();
            if (!WaitingOnPlayer)
            {
                LockstepTurnCounter++;
                PendingTurn?.NextTurn();
                CurrentTurn?.NextTurn();
                CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            }
        }
        // Turn 1
        else
        {
            LockstepTurnCounter++;
            CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            CurrentTurn.NextTurn();
        }

        Debug.Log($"Lockstep Update {System.DateTime.Now.Hour} hr / {System.DateTime.Now.Minute} min / {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
    }

    public void ReconnectedLockstepTurn()
    {
        // Turn 3 and above
        if (PendingTurn != null && ConfirmedTurn != null)
        {
            WaitingOnPlayer = !PendingTurn.ReadyForNextTurn() || !ConfirmedTurn.ReadyForNextTurn();
            if (!WaitingOnPlayer)
            {
                LockstepTurnCounter++;
                ConfirmedTurn?.NextTurn();
                PendingTurn?.NextTurn();
                CurrentTurn?.NextTurn();
                CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            }
        }
        // Turn 2
        else if (PendingTurn != null)
        {
            WaitingOnPlayer = !PendingTurn.ReadyForNextTurn();
            if (!WaitingOnPlayer)
            {
                LockstepTurnCounter++;
                PendingTurn?.NextTurn();
                CurrentTurn?.NextTurn();
                CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            }
        }
        // Turn 1
        else
        {
            LockstepTurnCounter++;
            CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            CurrentTurn.NextTurn();
        }
    }

    // Completed on the Next Lockstep Turn after the Lockstep Turn
    public void ProcessTurn()
    {
        if (ProcessingTurn != null)
        {
            foreach(PlayerActions PlayerAction in ProcessingTurn.AllActionsDone)
            {
                foreach(IAction Action in PlayerAction.ActionsDone)
                {
                    Action.ProcessAction();
                }
            }
            ProcessingTurn.NextTurn();
        }

        foreach (NetworkPlayer Player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            Player.ReadyForNextTurn = false;
        }
    }

    public void LocalPlayerActionDone(IAction ActionDone)
    {
        // Store the Local Player Actions so they can be sent off
        LocalPlayersCurrentTurn.AddAction(ActionDone);
    }

    public void SendOffLocalActions()
    {
        // Send off actions for current turn
        // Send it off - RPC call
        SendTurnActions();

        // Set New Current Actions
        LocalPlayersCurrentTurn = new PlayerActions(PlayerManager.Instance.LocalPlayer.PlayerID, true);
    }

    public void RecievePlayerAction(PlayerActions PendingActionToTrack)
    {
        PendingTurn.AllActionsDone.Add(PendingActionToTrack);

        if (PendingTurn.AllActionsDone.Count == PlayerManager.Instance.ConnectedPlayers.Count)
        {
            // Send Confirmation that you recieved all the player actions - RPC call
            SendConfirmation();
        }
    }

    public void SendTurnActions()
    {
        Debug.Log("Client Sent - Send actions");
        Message message = Message.Create(MessageSendMode.reliable, MessageId.SendTurnActions);
        int NumberOfActions = LocalPlayersCurrentTurn.ActionsDone.Count;
        message.AddInt(NumberOfActions);
        message.AddUShort(PlayerManager.Instance.LocalPlayer.PlayerID);

        foreach(IAction action in LocalPlayersCurrentTurn.ActionsDone)
        {
            AddActionToMessage(action, message);
        }

        NetworkManager.Instance.Client.Send(message);
    }

    private Message AddActionToMessage(IAction action, Message message)
    {
        message.AddInt(action.ActionType);
        if ((ActionTypes)action.ActionType == ActionTypes.NoAction)
        {
            // Thus far nothing needs to be added, may be changed later though 
        }

        return message;
    }

    [MessageHandler((ushort)MessageId.SendTurnActions)]
    private static void SendTurnActions(ushort fromClientId, Message message)
    {
        Debug.Log("Server Recieved - Send actions");
        int NumberOfActions = message.GetInt();
        ushort SentFromPlayerID = message.GetUShort();

        // ResendData
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.SendTurnActions);
        messageToSend.AddInt(NumberOfActions);
        messageToSend.AddUShort(SentFromPlayerID);
        PlayerActions PlayerAction = new PlayerActions(SentFromPlayerID);
        for (int actionIndex = 0; actionIndex < NumberOfActions; actionIndex++)
        {
            int TypeOfAction = message.GetInt();
            messageToSend.AddInt(TypeOfAction);

            IAction NewAction = null;

            if ((ActionTypes)TypeOfAction == ActionTypes.NoAction)
            {
                // For Now do nothing - Get And Add Variables for other actions
                NewAction = new NoAction();
            }
            else
            {
                NewAction = new CorruptAction();
            }

            if (NewAction != null)
            {
                NewAction.ActionType = TypeOfAction;
                NewAction.OwningPlayer = SentFromPlayerID;
                PlayerAction.AddAction(NewAction);
            }
        }

        Instance.RecievePlayerAction(PlayerAction);

        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            if (player == PlayerManager.Instance.LocalPlayer)
            {
                continue;
            }
            NetworkManager.Instance.Server.Send(messageToSend, player.PlayerID);
        }
    }

    [MessageHandler((ushort)MessageId.SendTurnActions)]
    private static void SendTurnActions(Message message)
    {
        if (NetworkManager.Instance.IsHost)
        {
            return;
        }
        Debug.Log("Cient Recieved - Send actions");
        int NumberOfActions = message.GetInt();
        ushort SentFromPlayerID = message.GetUShort();

        // ResendData
        PlayerActions PlayerAction = new PlayerActions(SentFromPlayerID);
        for (int actionIndex = 0; actionIndex < NumberOfActions; actionIndex++)
        {
            int TypeOfAction = message.GetInt();

            IAction NewAction = null;

            if ((ActionTypes)TypeOfAction == ActionTypes.NoAction)
            {
                // For Now do nothing - Get And Add Variables for other actions
                NewAction = new NoAction();
            }
            else
            {
                NewAction = new CorruptAction();
            }

            if (NewAction != null)
            {
                NewAction.ActionType = TypeOfAction;
                NewAction.OwningPlayer = SentFromPlayerID;
                PlayerAction.AddAction(NewAction);
            }
        }

        Instance.RecievePlayerAction(PlayerAction);
    }

    public void SendConfirmation()
    {
        Debug.Log("Send Sent - Confirmation");
        Message message = Message.Create(MessageSendMode.reliable, MessageId.TurnConfirmation);
        message.AddUShort(PlayerManager.Instance.LocalPlayer.PlayerID);
        message.AddBool(true);
        NetworkManager.Instance.Client.Send(message);
    }

    [MessageHandler((ushort)MessageId.TurnConfirmation)]
    private static void SendConfirmation(ushort fromClientId, Message message)
    {
        Debug.Log("Server Recieved - Send Confirmation");
        ushort newPlayerId = message.GetUShort();
        Message messageToSend = Message.Create(MessageSendMode.reliable, MessageId.TurnConfirmation);
        bool Confirmed = message.GetBool();
        messageToSend.AddUShort(newPlayerId);
        messageToSend.AddBool(Confirmed);

        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            NetworkManager.Instance.Server.Send(messageToSend, player.PlayerID);
        }
    }

    [MessageHandler((ushort)MessageId.TurnConfirmation)]
    private static void SendConfirmation(Message message)
    {
        Debug.Log("Client Recieved - Send Confirmation");
        ushort confirmedPlayer = message.GetUShort();
        bool Confirmed = message.GetBool();

        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            if (player.PlayerID == confirmedPlayer)
            {
                player.ReadyForNextTurn = Confirmed;
            }
        }
    }
}

[System.Serializable]
public class ActionTurn
{
    public ActionStates CurrentState = ActionStates.New;

    public List<PlayerActions> AllActionsDone = new List<PlayerActions>();

    public int LockStepTurnNumber = -1;
    public int GameTurnNumber = -1;
    public int CompletedOnGameTurn = -1;

    public ActionTurn(int LockStepNumber = -1, int GameTurn = -1)
    {
        LockStepTurnNumber = LockStepNumber;
        GameTurnNumber = GameTurn;
    }

    public bool ReadyForNextTurn()
    {
        if (CurrentState == ActionStates.New)
        {
            return true;
        }
        else if (CurrentState == ActionStates.Pending)
        {
            // Recieved Everyones Actions
            return PlayerManager.Instance.ConnectedPlayers.Count == AllActionsDone.Count;
        }
        else if (CurrentState == ActionStates.Confirmed)
        {
            CurrentState = ActionStates.Processing;
            // Process Everyone Turn
            foreach(NetworkPlayer Player in PlayerManager.Instance.ConnectedPlayers.Values)
            {
                if (!Player.ReadyForNextTurn)
                {
                    return false;
                }
            }

            return true;
        }
        else if (CurrentState == ActionStates.Processing)
        {
            return true;
        }

        return false;
    }

    public ActionTurn NextTurn()
    {
        if (CurrentState == ActionStates.New)
        {
            CurrentState = ActionStates.Pending;
            // Send Off All Actions
            LockstepManager.Instance.SendOffLocalActions();
        }
        else if (CurrentState == ActionStates.Pending)
        {
            // Recieved Everyones Actions
            CurrentState = ActionStates.Confirmed;
        }
        else if (CurrentState == ActionStates.Confirmed)
        {
            CurrentState = ActionStates.Processing;
            // Process Everyone Turn
            LockstepManager.Instance.ProcessTurn();
        }
        else if (CurrentState == ActionStates.Processing)
        {
            CurrentState = ActionStates.Completed;
            // Move into Turn History
            CompletedOnGameTurn = LockstepManager.Instance.FixedGameTurnCounter;
            LockstepManager.Instance.ActionTurnHistory.CompleteTurn(this);
        }

        return this;
    }
}

[System.Serializable]
public class TurnHistory
{
    public List<ActionTurn> AllTurnsCompleted = new List<ActionTurn>();

    public void CompleteTurn(ActionTurn CompletedTurn)
    {
        AllTurnsCompleted.Add(CompletedTurn);
    }

    public TurnHistory()
    {

    }
}

[System.Serializable]
public class PlayerActions
{
    public ushort PlayerID { get; set; }
    public int TurnNumber { get; set; }
    public List<IAction> ActionsDone = new List<IAction>();
    public static int TurnCounter = 0;

    public void AddAction(IAction Action)
    {
        ActionsDone.Add(Action);
    }

    public PlayerActions(ushort playerID = 0, bool UpdateTurnCounter = false)
    {
        if (UpdateTurnCounter)
        {
            TurnCounter++;
        }
        TurnNumber = TurnCounter;
        PlayerID = playerID;

        AddAction(new NoAction());
    }
}

public enum ActionStates
{
    New,
    Pending,
    Confirmed,
    Processing,
    Completed,
}

public enum ActionTypes : int
{
    Corrupt = 0,
    NoAction = 1,
}

[System.Serializable]
public class NoAction : IAction
{
    public int ActionType { get; set; }
    public ushort OwningPlayer { get; set; }

    public NoAction()
    {
        ActionType = (int)ActionTypes.NoAction;
    }

    public void ProcessAction()
    {
        // Do Nothing
    }
}

[System.Serializable]
public class CorruptAction : IAction
{
    public int ActionType { get; set; }
    public ushort OwningPlayer { get; set; }

    public CorruptAction()
    {
        ActionType = (int)ActionTypes.Corrupt;
    }

    public void ProcessAction()
    {
        // Something went wrong, post error
        Debug.LogError("Did not recieve action correctly, action was corrupted");
    }
}

public interface IAction
{
    public int ActionType { get; set; }
    public ushort OwningPlayer { get; set; }

    public void ProcessAction();
}
