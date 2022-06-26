using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet;
using FishNet.Object;

[System.Runtime.InteropServices.Guid("3749176F-32B4-4DB0-9BA8-F4493743DA0B")]
public class LockstepManager : MonoBehaviour
{
    [SerializeField]
    private bool TryToResendOnWait = false;
    [SerializeField]
    [Tooltip("Game Turn / ms")]
    private float AttemptGameTurnEvery = 40; // ms
    [SerializeField]
    private int KeepAveragePingOverSeconds = 10;
    public ActionTurn CurrentTurn { get; set; }
    public ActionTurn PendingTurn { get; set; }
    public ActionTurn ConfirmedTurn { get; set; }
    public ActionTurn ProcessingTurn { get; set; }
    public TurnHistory ActionTurnHistory = new TurnHistory();


    public static LockstepManager Instance = null;
    private int NumberOfGameTurnsPerLockstepTurn = 4; // Adjusts based on the connection ei: average time per round trip = 167, game now does 5 game turns per lockstep (Math.Ciel(167/40) = 5)
    private int NumberOfFixedUpdatesPerGameTurn = 2;
    private float WaitTime = 0;
    private bool ReconnectOnNextGameTurn = false;
    private bool CountDown = false;

    public int GameTurnCounter { get; set; }
    public int SecondsTillReconnect { get; set; }
    public long ReconnectOnSecond { get; set; }

    public int FixedFrameCounter { get; private set; }
    public int FixedGameTurnCounter { get; private set; }
    public int LockstepTurnCounter { get; private set; }

    public bool Reconnecting { get; set; }

    public bool SimulationStarted { get; set; }

    public bool SimulationPaused { get; set; }

    public bool WaitingOnPlayer { get; set; }
    public PlayerActions PreviousLocalPlayersCurrentTurn { get; set; }
    public PlayerActions LocalPlayersCurrentTurn { get; set; }
    public PlayerActions AiPreviousTurn { get; set; }
    public PlayerActions AiCurrentTurn { get; set; }
    public List<PlayerActions> ActionPendingList { get; set; }
    private int LastAskForResentSec = -1;
    private float GameTurnHalfTime = 0;
    private float GameTurnTimer = 0;
    public float HalfStepTime { get; set; }
    public float StepTime { get; set; }
    private float ResendConfirmationTImer = 0;

    private void Awake()
    {
        ActionPendingList = new List<PlayerActions>();
        Instance = this;
        FixedFrameCounter = 0;
        FixedGameTurnCounter = 0;
        LockstepTurnCounter = 0;
        PendingTurn = null;
        ConfirmedTurn = null;
        ProcessingTurn = null;
        GameTurnHalfTime = AttemptGameTurnEvery / 2000;
        HalfStepTime = GameTurnHalfTime;
        StepTime = 2 * HalfStepTime;

        HostSystemTimeDifference = -1;
        AveragePingTally = 0;
        AveragePingNumber = KeepAveragePingOverSeconds;
        AveragePingQueue = new Queue<int>(AveragePingNumber);
    }

    public void ResetManager()
    {
        ActionPendingList = new List<PlayerActions>();
        FixedFrameCounter = 0;
        FixedGameTurnCounter = 0;
        LockstepTurnCounter = 0;
        PendingTurn = null;
        ConfirmedTurn = null;
        ProcessingTurn = null;
        GameTurnHalfTime = AttemptGameTurnEvery / 2000;
        HalfStepTime = GameTurnHalfTime;
        StepTime = 2 * HalfStepTime;

        HostSystemTimeDifference = -1;
        AveragePingTally = 0;
        AveragePingNumber = KeepAveragePingOverSeconds;
    }

    private void Start()
    {
        InitializeBroadcasts();
    }

    private void Update()
    {
        if (WaitingOnPlayer)
        {
            WaitTime += Time.deltaTime;
            if (LockstepTurnCounter > 1)
            {
                WaitingOnPlayer = !ConfirmedTurn.ReadyForNextTurn();
                ResendConfirmationTImer += Time.deltaTime;
                if (ResendConfirmationTImer >= 0.08f)
                {
                    ConfirmedTurn.Reconfirm();
                }
            }
            // Turn 2
            else if (LockstepTurnCounter > 0)
            {
                WaitingOnPlayer = !PendingTurn.ReadyForNextTurn();
            }
            if (!WaitingOnPlayer)
            {
                Debug.Log($"wait Time {WaitTime}");
                WaitTime = 0;
            }
            LastAskForResentSec = Mathf.FloorToInt(WaitTime);
            //Reconnecting = !WaitingOnPlayer;

            //if (TryToResendOnWait && WaitingOnPlayer && LastAskForResentSec != Mathf.FloorToInt(WaitTime))
            //{
            //    LastAskForResentSec = Mathf.FloorToInt(WaitTime);
            //    List<ushort> WaitingOnPlayerIndexs = GetWaitingPlayersID(); 
            //    foreach(ushort playerID in WaitingOnPlayerIndexs)
            //    {
            //        if (playerID == PlayerManager.Instance.LocalPlayer.PlayerID)
            //        {
            //            ResendTurnActions();
            //        }
            //        else
            //        {
            //            SendForActionResend(playerID);
            //        }
            //    }
            //}
            //else
            //{
            //    LastAskForResentSec = -1;
            //}
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
                ReconnectOnSecond = System.DateTime.Now.Ticks + 1600000 - LastPing / 2; // seconds to 100 nano seconds ~ 10*7
                SecondsTillReconnect = 2;
            }
            else
            {
                // Large Disconnect
                CountDown = true;
                ReconnectOnNextGameTurn = true;
                ReconnectOnSecond = System.DateTime.Now.Ticks + 100000000 - LastPing / 2; // seconds to 100 nano seconds ~ 10*7
                SecondsTillReconnect = 10;
            }
            WaitTime = 0;
            Reconnecting = false;
        }
        else if (!SimulationStarted)
        {
            if (PlayerManager.Instance.EveryoneIsReadyForStart() || PlayerManager.Instance.OfflineGameStarted)
            {
                CountDown = true;
                //if (NetworkManager.Instance.IsHost)
                //{
                //    SendCountdown(ReconnectOnSecond);
                //}
                SecondsTillReconnect = 6;

                SimulationStarted = true;

                if (PlayerManager.Instance.ActiveOnlineMode == PlayModes.None || PlayerManager.Instance.ActiveOnlineMode == PlayModes.VsComputer)
                {
                    ReconnectOnSecond = System.DateTime.Now.Ticks + 6 * 10000000;
                    AiCurrentTurn = new PlayerActions(2, false);
                    LocalPlayersCurrentTurn = new PlayerActions(1, true);
                    PlayerManager.Instance.OfflineGameStarted = false;
                }
                else
                {
                    ReconnectOnSecond = System.DateTime.Now.Ticks + 6 * 10000000 - LastPing / 2; // seconds to 100 nano seconds ~ 10*7
                    LocalPlayersCurrentTurn = new PlayerActions(PlayerManager.Instance.LocalPlayer.PlayerID, true);
                }
            }
        }

        if (CountDown)
        {
            if (SecondsTillReconnect <= -1)
            {
                CountDown = false;
                SecondsTillReconnect = 0;
                //Debug.Log($"Finished Countdown on -> {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
            }
            else
            {
                SecondsTillReconnect = Mathf.FloorToInt((ReconnectOnSecond - System.DateTime.Now.Ticks) / 10000000);
            }
        }

        GameUpdate();
    }

    private List<ushort> GetWaitingPlayersID()
    {
        List<ushort> WaitingOnPLayers = new List<ushort>();
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            bool ContainsPlayerID = false;
            foreach (PlayerActions action in ActionPendingList)
            {
                if (action.PlayerID == player.PlayerID)
                {
                    ContainsPlayerID = true;
                    break;
                }
            }

            if (!ContainsPlayerID)
            {
                WaitingOnPLayers.Add(player.PlayerID);
            }
        }

        return WaitingOnPLayers;
    }

    private void GameUpdate()
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
            return;
        }

        GameTurnTimer += Time.deltaTime;
        while (GameTurnTimer >= GameTurnHalfTime)
        {
            //Debug.Log($"Fixed Delta Times ({Time.fixedDeltaTime})");
            GameTurnTimer -= GameTurnHalfTime;
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
    }

    public void AsyncGameTurn()
    {
        BaseUnitBehaviour.UpdateUnits();
        AiController.UpdateAiControllers();

        PingHost();
        //Debug.Log($"Async Update {System.DateTime.Now.Hour} hr / {System.DateTime.Now.Minute} min / {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
    }

    public void GameTurn()
    {
        GameTurnCounter++;
        if (ReconnectOnNextGameTurn)
        {
            ReconnectedLockstepTurn();
            ReconnectOnNextGameTurn = false;
        }

        FixedGameTurnCounter++;
        if (FixedGameTurnCounter == NumberOfGameTurnsPerLockstepTurn)
        {
            FixedGameTurnCounter = 0;
            LockstepTurn();
        }

        BaseBuilding.UpdateBases();
        BaseUnitBehaviour.UpdateUnits();
        BuyUnitUI.UpdateAllBuyUi();

        // Update your ai last so it gets the upto date info of the game state
        GameAi.AskAllAiForDecision();
        //Debug.Log($"Game Update -> {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
    }

    public void LockstepTurn()
    {
        // Turn 3 and above
        if (LockstepTurnCounter > 1)
        {
            WaitingOnPlayer = !ConfirmedTurn.ReadyForNextTurn();
            if (!WaitingOnPlayer)
            {
                //Debug.Log($"Turn Number {LockstepTurnCounter} Lockstep Update at Time: {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
                LockstepTurnCounter++;
                ConfirmedTurn?.NextTurn();
                PendingTurn?.NextTurn();
                CurrentTurn?.NextTurn();
                CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            }
        }
        // Turn 2
        else if (LockstepTurnCounter > 0)
        {
            WaitingOnPlayer = !PendingTurn.ReadyForNextTurn();
            if (!WaitingOnPlayer)
            {
                //Debug.Log($"Turn Number {LockstepTurnCounter} Lockstep Update at Time: {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
                LockstepTurnCounter++;
                PendingTurn?.NextTurn();
                CurrentTurn?.NextTurn();
                CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            }
        }
        // Turn 1
        else
        {
            //Debug.Log($"Turn Number {LockstepTurnCounter} Lockstep Update at Time: {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
            CurrentTurn = new ActionTurn(LockstepTurnCounter, 0);
            LockstepTurnCounter++;
            CurrentTurn.NextTurn();
            CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
        }
    }

    public void ReconnectedLockstepTurn()
    {
        // Turn 3 and above
        if (LockstepTurnCounter > 1)
        {
            WaitingOnPlayer = !ConfirmedTurn.ReadyForNextTurn();
            if (!WaitingOnPlayer)
            {
                //Debug.Log($"Turn Number {LockstepTurnCounter} Reconnect Lockstep Update at Time: {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
                LockstepTurnCounter++;
                ConfirmedTurn?.NextTurn();
                PendingTurn?.NextTurn();
                CurrentTurn?.NextTurn();
                CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            }
        }
        // Turn 2
        else if (LockstepTurnCounter > 0)
        {
            WaitingOnPlayer = !PendingTurn.ReadyForNextTurn();
            if (!WaitingOnPlayer)
            {
                //Debug.Log($"Turn Number {LockstepTurnCounter} Reconnect Lockstep Update at Time: {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
                LockstepTurnCounter++;
                PendingTurn?.NextTurn();
                CurrentTurn?.NextTurn();
                CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
            }
        }
        // Turn 1
        else
        {
            //Debug.Log($"Turn Number {LockstepTurnCounter} Reconnect Lockstep Update at Time: {System.DateTime.Now.Second} sec / {System.DateTime.Now.Millisecond} ms");
            CurrentTurn = new ActionTurn(LockstepTurnCounter, 0);
            LockstepTurnCounter++;
            CurrentTurn.NextTurn();
            CurrentTurn = new ActionTurn(LockstepTurnCounter, GameTurnCounter);
        }
    }

    // Completed on the Next Lockstep Turn after the Lockstep Turn
    public void ProcessTurn()
    {
        if (ProcessingTurn != null)
        {
            if (ProcessingTurn.AllActionsDone == null)
            {
                Debug.LogWarning($"No Action List Created for lockstep turn: {ProcessingTurn.LockStepTurnNumber}");
                ProcessingTurn.NextTurn();
                return;
            }
            foreach(PlayerActions PlayerAction in ProcessingTurn.AllActionsDone)
            {
                foreach(IAction Action in PlayerAction.ActionsDone)
                {
                    Action.ProcessAction();
                }
            }
            ProcessingTurn.NextTurn();
        }
    }

    public void LocalPlayerActionDone(IAction ActionDone)
    {
        // Store the Local Player Actions so they can be sent off
        LocalPlayersCurrentTurn.AddAction(ActionDone);
    }

    public void AddAction(IAction ActionToAdd)
    {
        LocalPlayersCurrentTurn.AddAction(ActionToAdd);
    }

    public void AddAiAction(IAction ActionToAdd)
    {
        AiCurrentTurn.AddAction(ActionToAdd);
    }

    public void SendOffLocalActions()
    {
        if (PlayerManager.Instance.ActiveOnlineMode == PlayModes.Ranked || PlayerManager.Instance.ActiveOnlineMode == PlayModes.Ranked || PlayerManager.Instance.ActiveOnlineMode == PlayModes.Ranked)
        {
            // Send off actions for current turn
            // Send it off - RPC call
            SendTurnActions();

            // Set New Current Actions
            PreviousLocalPlayersCurrentTurn = LocalPlayersCurrentTurn;
            LocalPlayersCurrentTurn = new PlayerActions(PlayerManager.Instance.LocalPlayer.PlayerID, true);
        }
        else
        {
            // Offline MODE
            OfflineRecievePlayerAction(LocalPlayersCurrentTurn, AiCurrentTurn);

            // Set New Current Actions
            AiPreviousTurn = AiCurrentTurn;
            AiCurrentTurn = new PlayerActions(2, false);

            PreviousLocalPlayersCurrentTurn = LocalPlayersCurrentTurn;
            LocalPlayersCurrentTurn = new PlayerActions(1, true);
        }
    }

    public void OfflineRecievePlayerAction(PlayerActions PlayerAction, PlayerActions AiAction)
    {
        if (CurrentTurn != null && CurrentTurn.LockStepTurnNumber == PlayerAction.TurnNumber)
        {
            CurrentTurn.AddActionSet(PlayerAction);
            CurrentTurn.AddActionSet(AiAction);
        }
        else if (PendingTurn != null && PendingTurn.LockStepTurnNumber == PlayerAction.TurnNumber)
        {
            CurrentTurn.AddActionSet(PlayerAction);
            CurrentTurn.AddActionSet(AiAction);
        }
        else if (ConfirmedTurn != null && ConfirmedTurn.LockStepTurnNumber == PlayerAction.TurnNumber)
        {
            CurrentTurn.AddActionSet(PlayerAction);
            CurrentTurn.AddActionSet(AiAction);
        }
    }

    public void RecievePlayerAction(PlayerActions PendingActionToTrack)
    {
        ActionPendingList.Add(PendingActionToTrack);
        if (CurrentTurn != null && CurrentTurn.LockStepTurnNumber == PendingActionToTrack.TurnNumber)
        {
            CurrentTurn.CheckForActions();
        }
        else if (PendingTurn != null && PendingTurn.LockStepTurnNumber == PendingActionToTrack.TurnNumber)
        {
            PendingTurn.CheckForActions();
        }
        else if (ConfirmedTurn != null && ConfirmedTurn.LockStepTurnNumber == PendingActionToTrack.TurnNumber)
        {
            ConfirmedTurn.CheckForActions();
        }
    }

    public bool PendingTurnContainActionsFromAllPlayers(int ForTurnNumber)
    {
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            if (!ContainsActionFromPlayer(player.PlayerID, ForTurnNumber))
            {
                return false;
            }
        }
        return true;
    }

    public bool ContainsActionFromPlayer(int PlayerID, int ForTurnNumber)
    {
        foreach (PlayerActions action in ActionPendingList)
        {
            if (action.TurnNumber == ForTurnNumber)
            {
                if (action.PlayerID == PlayerID)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool BroadcastsInitialized = false;
    public void InitializeBroadcasts()
    {
        if (!BroadcastsInitialized)
        {
            BroadcastsInitialized = true;
            /// Actions
            // No Action
            InstanceFinder.NetworkManager.ServerManager.RegisterBroadcast<NoActionBroadcast>(NoActionBroadcast_ToServer);
            InstanceFinder.NetworkManager.ClientManager.RegisterBroadcast<NoActionBroadcast>(NoActionBroadcast_ToClients);
            // Corrupted Action
            InstanceFinder.NetworkManager.ServerManager.RegisterBroadcast<CorruptActionBroadcast>(CorruptActionBroadcast_ToServer);
            InstanceFinder.NetworkManager.ClientManager.RegisterBroadcast<CorruptActionBroadcast>(CorruptActionBroadcast_ToClients);
            // Buy Unit Action
            InstanceFinder.NetworkManager.ServerManager.RegisterBroadcast<BuyUnitActionBroadcast>(BuyUnitActionBroadcast_ToServer);
            InstanceFinder.NetworkManager.ClientManager.RegisterBroadcast<BuyUnitActionBroadcast>(BuyUnitActionBroadcast_ToClients);

            /// Action Confirmation
            InstanceFinder.NetworkManager.ServerManager.RegisterBroadcast<ActionConfirmationBroadcast>(ActionConfirmationBroadcast_ToServer);
            InstanceFinder.NetworkManager.ClientManager.RegisterBroadcast<ActionConfirmationBroadcast>(ActionConfirmationBroadcast_ToClients);
            /// Coundown
            InstanceFinder.NetworkManager.ServerManager.RegisterBroadcast<CountdownBroadcast>(CountdownBroadcast_ToServer);
            InstanceFinder.NetworkManager.ClientManager.RegisterBroadcast<CountdownBroadcast>(CountdownBroadcast_ToClients);
            /// Ping
            InstanceFinder.NetworkManager.ServerManager.RegisterBroadcast<PingBroadcast>(PingHost_ToServer);
            InstanceFinder.NetworkManager.ClientManager.RegisterBroadcast<PingBroadcast>(PingHost_ToClients);
        }
    }

    #region Send Actions RPC
    public void SendTurnActions()
    {
        //Debug.Log("Client Sent - Send actions");
        int NumberOfActions = LocalPlayersCurrentTurn.ActionsDone.Count;
        //Debug.Log($"Sending {NumberOfActions} Actions");
        foreach (IAction action in LocalPlayersCurrentTurn.ActionsDone)
        {
            SendActionBroadcast(action, LocalPlayersCurrentTurn.TurnNumber, NumberOfActions, PlayerManager.Instance.LocalPlayer.PlayerID);
        }
    }

    public void ResendTurnActions()
    {
        //Debug.Log("Client Sent - Send actions");
        int NumberOfActions = PreviousLocalPlayersCurrentTurn.ActionsDone.Count;
        foreach (IAction action in PreviousLocalPlayersCurrentTurn.ActionsDone)
        {
            SendActionBroadcast(action, PreviousLocalPlayersCurrentTurn.TurnNumber, NumberOfActions, PlayerManager.Instance.LocalPlayer.PlayerID);
        }
    }

    #region Riptide

    private Message AddActionToMessage(IAction action, Message message)
    {
        message.AddInt(action.ActionType);
        if ((ActionTypes)action.ActionType == ActionTypes.NoAction)
        {
            // Thus far nothing needs to be added, may be changed later though 
        }
        else if ((ActionTypes)action.ActionType == ActionTypes.BuyUnit)
        {
            // Buy Action needs to add an int for the buy index
            BuyUnitAction BuyAction = action as BuyUnitAction;
            message.AddInt(BuyAction.UnitBuyIndex);
        }

        return message;
    }

    [MessageHandler((ushort)AOW.RiptideNetworking.MessageId.SendTurnActions)]
    private static void SendTurnActions(ushort fromClientId, Message message)
    {
        //Debug.Log("Server Recieved - Send actions");
        int TurnNumber = message.GetInt();
        int NumberOfActions = message.GetInt();
        ushort SentFromPlayerID = message.GetUShort();

        // ResendData
        Message messageToSend = Message.Create(MessageSendMode.reliable, AOW.RiptideNetworking.MessageId.SendTurnActions);
        messageToSend.AddInt(TurnNumber);
        messageToSend.AddInt(NumberOfActions);
        messageToSend.AddUShort(SentFromPlayerID);

        int TypeOfAction = message.GetInt();
        messageToSend.AddInt(TypeOfAction);

        if ((ActionTypes)TypeOfAction == ActionTypes.NoAction)
        {
            // For Now do nothing - Get And Add Variables for other actions
            //NewAction = new NoAction();
        }
        else if ((ActionTypes)TypeOfAction == ActionTypes.BuyUnit)
        {
            // Buy Action needs to add an int for the buy index
            //NewAction = action as BuyUnitAction;
            //message.AddInt(BuyAction.UnitBuyIndex);
            int buyIndex = message.GetInt();
            messageToSend.AddInt(buyIndex);
        }
        else
        {
            // Corrupted or Unkown action
        }

        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            AOW.RiptideNetworking.NetworkManager.Instance.Server.Send(messageToSend, player.PlayerID);
        }
    }

    private static Dictionary<(ushort, int), PlayerActions> PartitioningActions = new Dictionary<(ushort, int), PlayerActions>();
    [MessageHandler((ushort)AOW.RiptideNetworking.MessageId.SendTurnActions)]
    private static void SendTurnActions(Message message)
    {
        int TurnNumber = message.GetInt();
        int NumberOfActions = message.GetInt();
        ushort SentFromPlayerID = message.GetUShort();
        //Debug.Log($"Turn Number {TurnNumber} From {SentFromPlayerID} For Actions RPC");

        PlayerActions PlayerAction;
        if (PartitioningActions.ContainsKey((SentFromPlayerID, TurnNumber)))
        {
            PlayerAction = PartitioningActions[(SentFromPlayerID, TurnNumber)];
        }
        else
        {
            PlayerAction = new PlayerActions(SentFromPlayerID, false, false);
            PlayerAction.TurnNumber = TurnNumber;
        }


        int TypeOfAction = message.GetInt();

        IAction NewAction;

        if ((ActionTypes)TypeOfAction == ActionTypes.NoAction)
        {
            // For Now do nothing - Get And Add Variables for other actions
            NewAction = new NoAction();
            NewAction.OwningPlayer = SentFromPlayerID;
        }
        else if ((ActionTypes)TypeOfAction == ActionTypes.BuyUnit)
        {
            // Buy Action needs to add an int for the buy index
            int buyIndex = message.GetInt();
            NewAction = new BuyUnitAction(buyIndex);
            NewAction.OwningPlayer = SentFromPlayerID;
        }
        else
        {
            NewAction = new CorruptAction();
            NewAction.OwningPlayer = SentFromPlayerID;
        }

        if (NewAction != null)
        {
            NewAction.ActionType = TypeOfAction;
            NewAction.OwningPlayer = SentFromPlayerID;
            if ((ActionTypes)TypeOfAction != ActionTypes.NoAction)
            {
                Debug.Log($"Add Action '{(ActionTypes)TypeOfAction}' on Turn {TurnNumber} -> Action Count {NumberOfActions} -> Player {SentFromPlayerID}");
            }
            PlayerAction.AddAction(NewAction);
        }

        if (NumberOfActions > 1 && NumberOfActions != PlayerAction.ActionsDone.Count && !PartitioningActions.ContainsKey((SentFromPlayerID, TurnNumber)))
        {
            //Debug.Log($"Add To Partition -> Turn {TurnNumber} -> Action Count {NumberOfActions} -> Player {SentFromPlayerID}");
            PartitioningActions.Add((SentFromPlayerID, TurnNumber), PlayerAction);
        }

        if (NumberOfActions == PlayerAction.ActionsDone.Count)
        {
            //Debug.Log($"Turn {TurnNumber} Recieved Action Set for player {SentFromPlayerID} -> Action Count {NumberOfActions} -> Player {SentFromPlayerID}");
            PartitioningActions.Remove((SentFromPlayerID, TurnNumber));
            Instance.RecievePlayerAction(PlayerAction);
        }
        //else
        //{
        //    Debug.Log($"Logged {PlayerAction.ActionsDone.Count} of {NumberOfActions} on Turn {TurnNumber} -> Action Count {NumberOfActions} -> Player {SentFromPlayerID}");
        //}
    }
    #endregion

    #region Fishnet


    public void SendActionBroadcast(IAction action, int Turn, int ActionCount, ushort SentBy)
    {
        if ((ActionTypes)action.ActionType == ActionTypes.NoAction)
        {
            // Thus far nothing needs to be added, may be changed later though 
            NoActionBroadcast Broadcast = new NoActionBroadcast()
            {
                TurnNumber = Turn,
                NumberOfActions = ActionCount,
                SentByPlayer = SentBy,
            };

            InstanceFinder.ClientManager.Broadcast(Broadcast);
        }
        else if ((ActionTypes)action.ActionType == ActionTypes.Corrupt)
        {
            // Corrupted action
            CorruptActionBroadcast Broadcast = new CorruptActionBroadcast()
            {
                TurnNumber = Turn,
                NumberOfActions = ActionCount,
                SentByPlayer = SentBy,
            };

            InstanceFinder.ClientManager.Broadcast(Broadcast);
        }
        else if ((ActionTypes)action.ActionType == ActionTypes.BuyUnit)
        {
            // Buy Action needs to add an int for the buy index
            BuyUnitAction BuyAction = action as BuyUnitAction;
            BuyUnitActionBroadcast Broadcast = new BuyUnitActionBroadcast()
            {
                TurnNumber = Turn,
                NumberOfActions = ActionCount,
                SentByPlayer = SentBy,
                BuyIndex = BuyAction.UnitBuyIndex,
            };

            //Debug.Log($"Sending {ActionCount} Actions 2");
            InstanceFinder.ClientManager.Broadcast(Broadcast);
        }
    }

    public void NoActionBroadcast_ToServer(NetworkConnection conn, NoActionBroadcast broadcast)
    {
        NetworkObject nob = conn.FirstObject;
        if (nob == null)
        {
            return;
        }
        //Debug.Log($"Sending {broadcast.NumberOfActions} Actions 3");
        InstanceFinder.ServerManager.Broadcast(nob, broadcast);
    }

    public void NoActionBroadcast_ToClients(NoActionBroadcast broadcast)
    {
        NoAction NewAction = new NoAction();
        NewAction.OwningPlayer = broadcast.SentByPlayer;
        RecieveAction(NewAction, broadcast.SentByPlayer, broadcast.TurnNumber, NewAction.ActionType, broadcast.NumberOfActions);
    }

    public void CorruptActionBroadcast_ToServer(NetworkConnection conn, CorruptActionBroadcast broadcast)
    {
        NetworkObject nob = conn.FirstObject;
        if (nob == null)
        {
            return;
        }
        InstanceFinder.ServerManager.Broadcast(nob, broadcast);
    }

    public void CorruptActionBroadcast_ToClients(CorruptActionBroadcast broadcast)
    {
        CorruptAction NewAction = new CorruptAction();
        NewAction.OwningPlayer = broadcast.SentByPlayer;
        RecieveAction(NewAction, broadcast.SentByPlayer, broadcast.TurnNumber, NewAction.ActionType, broadcast.NumberOfActions);
    }

    public void BuyUnitActionBroadcast_ToServer(NetworkConnection conn, BuyUnitActionBroadcast broadcast)
    {
        NetworkObject nob = conn.FirstObject;
        if (nob == null)
        {
            return;
        }
        InstanceFinder.ServerManager.Broadcast(nob, broadcast);
    }

    public void BuyUnitActionBroadcast_ToClients(BuyUnitActionBroadcast broadcast)
    {
        BuyUnitAction NewAction = new BuyUnitAction();
        NewAction.UnitBuyIndex = broadcast.BuyIndex;
        NewAction.OwningPlayer = broadcast.SentByPlayer;
        RecieveAction(NewAction, broadcast.SentByPlayer, broadcast.TurnNumber, NewAction.ActionType, broadcast.NumberOfActions);
    }

    public void RecieveAction(IAction NewAction, ushort SentFromPlayerID, int TurnNumber, int TypeOfAction, int NumberOfActions)
    {
        PlayerActions PlayerAction;
        if (PartitioningActions.ContainsKey((SentFromPlayerID, TurnNumber)))
        {
            PlayerAction = PartitioningActions[(SentFromPlayerID, TurnNumber)];
        }
        else
        {
            PlayerAction = new PlayerActions(SentFromPlayerID, false, false);
            PlayerAction.TurnNumber = TurnNumber;
        }

        if (NewAction != null)
        {
            //if ((ActionTypes)TypeOfAction != ActionTypes.NoAction)
            //{
                //Debug.Log($"Add Action '{(ActionTypes)TypeOfAction}' on Turn {TurnNumber} -> Action Count {NumberOfActions} -> Player {SentFromPlayerID}");
            //}
            PlayerAction.AddAction(NewAction);
        }
        //Debug.Log($"Recieved Action From Player {SentFromPlayerID} -> Action Type {(ActionTypes)NewAction.ActionType} -> Turn Number {TurnNumber} -> Number of Actions {NumberOfActions}");

        if (NumberOfActions > 1 && NumberOfActions != PlayerAction.ActionsDone.Count && !PartitioningActions.ContainsKey((SentFromPlayerID, TurnNumber)))
        {
            //Debug.Log($"Add To Partition -> Turn {TurnNumber} -> Action Count {NumberOfActions} -> Player {SentFromPlayerID}");
            PartitioningActions.Add((SentFromPlayerID, TurnNumber), PlayerAction);
        }

        if (NumberOfActions == PlayerAction.ActionsDone.Count)
        {
            //Debug.Log($"Turn {TurnNumber} Recieved Action Set for player {SentFromPlayerID} -> Action Count {NumberOfActions} -> Player {SentFromPlayerID}");
            PartitioningActions.Remove((SentFromPlayerID, TurnNumber));
            Instance.RecievePlayerAction(PlayerAction);
        }
    }

    #endregion

    #endregion

    #region Send Action Confirmation RPC

    public void SendConfirmation(int ConfirmedTurnNumber)
    {
        //Debug.Log("Send Sent - Confirmation");
        ActionConfirmationBroadcast ConfirmationBroadcast = new ActionConfirmationBroadcast()
        { 
            TurnNumber = ConfirmedTurnNumber,
            SentByPlayer = PlayerManager.Instance.LocalPlayer.PlayerID,
        };

        InstanceFinder.ClientManager.Broadcast(ConfirmationBroadcast);
    }

    #region Fishnet

    public void ActionConfirmationBroadcast_ToServer(NetworkConnection conn, ActionConfirmationBroadcast broadcast)
    {
        NetworkObject nob = conn.FirstObject;
        if (nob == null)
        {
            return;
        }
        InstanceFinder.ServerManager.Broadcast(nob, broadcast);
    }

    public void ActionConfirmationBroadcast_ToClients(ActionConfirmationBroadcast broadcast)
    {
        if (PendingTurn.LockStepTurnNumber == broadcast.TurnNumber)
        {
            if (!PendingTurn.ConfirmedPlayers.Contains(broadcast.SentByPlayer))
            {
                PendingTurn.ConfirmedPlayers.Add(broadcast.SentByPlayer);
            }
        }
        else if (ConfirmedTurn.LockStepTurnNumber == broadcast.TurnNumber)
        {
            if (!ConfirmedTurn.ConfirmedPlayers.Contains(broadcast.SentByPlayer))
            {
                ConfirmedTurn.ConfirmedPlayers.Add(broadcast.SentByPlayer);
            }
        }
        else if (CurrentTurn.LockStepTurnNumber == broadcast.TurnNumber)
        {
            if (!CurrentTurn.ConfirmedPlayers.Contains(broadcast.SentByPlayer))
            {
                CurrentTurn.ConfirmedPlayers.Add(broadcast.SentByPlayer);
            }
        }
    }

    #endregion

    #endregion

    #region Send Countdown RPC
    public void SendCountdown(int Sec)
    {
        //Debug.Log("Sent - Reconnect Countdown Sec");
        CountdownBroadcast Broadcast = new CountdownBroadcast()
        {
            SentByPlayer = PlayerManager.Instance.LocalPlayer.PlayerID,
            ReconnectionSecond = Sec,
        };

        InstanceFinder.ClientManager.Broadcast(Broadcast);
    }

    #region Fishnet

    public void CountdownBroadcast_ToServer(NetworkConnection conn, CountdownBroadcast broadcast)
    {
        NetworkObject nob = conn.FirstObject;
        if (nob == null)
        {
            return;
        }
        InstanceFinder.ServerManager.Broadcast(nob, broadcast);
    }

    public void CountdownBroadcast_ToClients(CountdownBroadcast broadcast)
    {
        Instance.ReconnectOnSecond = broadcast.ReconnectionSecond;
    }

    #endregion

    #endregion

    #region Ask For Action Resend RPC

    public void SendForActionResend(ushort ForPlayer)
    {
        //Debug.Log("Send Sent - Confirmation");
    }

    #region Riptide

    [MessageHandler((ushort)AOW.RiptideNetworking.MessageId.RequestForActionResend)]
    private static void SendForActionResend(ushort fromClientId, Message message)
    {
        //Debug.Log("Server Recieved - Send Confirmation");
        ushort newPlayerId = message.GetUShort();
        ushort otherPlayer = message.GetUShort();
        Message messageToSend = Message.Create(MessageSendMode.reliable, AOW.RiptideNetworking.MessageId.RequestForActionResend);
        messageToSend.Add(newPlayerId);

        AOW.RiptideNetworking.NetworkManager.Instance.Server.Send(messageToSend, otherPlayer);
    }

    [MessageHandler((ushort)AOW.RiptideNetworking.MessageId.RequestForActionResend)]
    private static void SendForActionResend(Message message)
    {
        ushort confirmedPlayer = message.GetUShort();
        //Debug.Log($"Got Resend Request from Player {confirmedPlayer}");
        Instance.ResendTurnActions();
    }
    #endregion

    #endregion

    #region Ping RPC

    public long HostSystemTimeDifference { get; set; }
    public long LastPing { get; set; }
    public int LastPingMS { get; set; }
    public int AveragePingMS { get; set; } // Average ping over last ten seconds
    public Queue<int> AveragePingQueue { get; set; }
    public int AveragePingTally { get; set; }
    private long PingStartTime_ns = 0;
    private float PingTimer = 0;
    private int AveragePingNumber;
    private float TimePerTick;
    private float TickRateTimer = 0;
    public void PingHost(bool ReliableSend = false)
    {
        if (PlayerManager.Instance.ActiveOnlineMode == PlayModes.CustomGame || PlayerManager.Instance.ActiveOnlineMode == PlayModes.Ranked || PlayerManager.Instance.ActiveOnlineMode == PlayModes.Quickplay)
        {
            PingStartTime_ns = System.DateTime.Now.Ticks;
            PingBroadcast Ping = new PingBroadcast()
            {
                Reliable = ReliableSend,
                SentByPlayer = PlayerManager.Instance.LocalPlayer.PlayerID,
                ServerTimeNow = 0,
            };

            InstanceFinder.ClientManager.Broadcast(Ping);
        }
    }

    public void PingHost_ToServer(NetworkConnection conn, PingBroadcast broadcast)
    {
        NetworkObject nob = conn.FirstObject;
        if (nob == null)
        {
            return;
        }

        PingBroadcast PingResponse = new PingBroadcast()
        {
            Reliable = broadcast.Reliable,
            SentByPlayer = broadcast.SentByPlayer,
            ServerTimeNow = System.DateTime.Now.Ticks,
        };

        InstanceFinder.ServerManager.Broadcast(nob, PingResponse, true, broadcast.Reliable ? FishNet.Transporting.Channel.Reliable : FishNet.Transporting.Channel.Unreliable);
    }

    public void PingHost_ToClients(PingBroadcast broadcast)
    {
        if (broadcast.SentByPlayer != PlayerManager.Instance.LocalPlayer.PlayerID)
        {
            return;
        }
        long Host100NanoSec = broadcast.ServerTimeNow;

        // Time Difference from ping to ping back
        long Client100NanoSec = System.DateTime.Now.Ticks;
        LastPing = Client100NanoSec - PingStartTime_ns;
        LastPingMS = Mathf.RoundToInt((float)Instance.LastPing / 10000); // One MS per 10000 (100 - Nanoseconds)

        if (HostSystemTimeDifference == -1)
        {
            //                            (         Estimate Time when Ping was             )
            long EstimatedHostTime = Host100NanoSec - Instance.LastPing / 2;
            HostSystemTimeDifference = PingStartTime_ns - EstimatedHostTime;
            float MSDifference = Mathf.RoundToInt((float)HostSystemTimeDifference / 10000);
            Debug.Log($"Host/Client System MS Difference {MSDifference}");
        }

        AveragePingTally += LastPingMS;
        int RemovedValue = 0;
        if (AveragePingQueue.Count == AveragePingNumber)
        {
            RemovedValue = Instance.AveragePingQueue.Dequeue();
        }

        AveragePingTally -= RemovedValue;
        AveragePingQueue.Enqueue(Instance.LastPingMS);
        AveragePingMS = Instance.AveragePingTally / AveragePingNumber;
    }

    #endregion
}

[System.Serializable]
public class ActionTurn
{
    public ActionStates CurrentState = ActionStates.New;

    public List<PlayerActions> AllActionsDone = null;

    public int LockStepTurnNumber = -1;
    public int GameTurnNumber = -1;
    public int CompletedOnGameTurn = -1;
    public List<int> ConfirmedPlayers = new List<int>();

    public ActionTurn(int LockStepNumber = -1, int GameTurn = -1)
    {
        if (AllActionsDone == null)
        {
            AllActionsDone = new List<PlayerActions>();
        }
        LockStepTurnNumber = LockStepNumber;
        GameTurnNumber = GameTurn;
    }

    public void CheckForActions()
    {
        bool containsActionsFromEveryone = LockstepManager.Instance.PendingTurnContainActionsFromAllPlayers(LockStepTurnNumber);
        if (containsActionsFromEveryone)
        {
            List<PlayerActions> RemoveList = new List<PlayerActions>();
            foreach (PlayerActions action in LockstepManager.Instance.ActionPendingList)
            {
                if (action.TurnNumber == LockStepTurnNumber)
                {
                    RemoveList.Add(action);
                    AddActionSet(action);
                }
            }
            foreach (PlayerActions action in RemoveList)
            {
                if (action.TurnNumber == LockStepTurnNumber)
                {
                    LockstepManager.Instance.ActionPendingList.Remove(action);
                }
            }
            LockstepManager.Instance.SendConfirmation(LockStepTurnNumber);
        }
    }

    public bool ReadyForNextTurn()
    {
        if (PlayerManager.Instance.ActiveOnlineMode == PlayModes.None || PlayerManager.Instance.ActiveOnlineMode == PlayModes.VsComputer)
        {
            return true;
        }

        if (CurrentState == ActionStates.New)
        {
            return true;
        }
        else if (CurrentState == ActionStates.Pending)
        {
            // Recieved Everyones Actions
            bool containsActionsInTurnFromEveryone = ContainsActionsFromAllPlayers();
            if (containsActionsInTurnFromEveryone)
            {
                return containsActionsInTurnFromEveryone;
            }
            bool containsActionsFromEveryone = LockstepManager.Instance.PendingTurnContainActionsFromAllPlayers(LockStepTurnNumber);
            if (containsActionsFromEveryone)
            {
                List<PlayerActions> RemoveList = new List<PlayerActions>();
                foreach (PlayerActions action in LockstepManager.Instance.ActionPendingList)
                {
                    if (action.TurnNumber == LockStepTurnNumber)
                    {
                        RemoveList.Add(action);
                        AddActionSet(action);
                    }
                }
                foreach (PlayerActions action in RemoveList)
                {
                    if (action.TurnNumber == LockStepTurnNumber)
                    {
                        LockstepManager.Instance.ActionPendingList.Remove(action);
                    }
                }
                LockstepManager.Instance.SendConfirmation(LockStepTurnNumber);
            }
            return containsActionsFromEveryone;
        }
        else
        {
            bool EveryoneConfirmed = true;
            foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
            {
                bool ContainedPlayer = false;
                foreach (int playerIdConfirmed in ConfirmedPlayers)
                {
                    if (player.PlayerID == playerIdConfirmed)
                    {
                        ContainedPlayer = true;
                        break;
                    }
                }

                if (!ContainedPlayer)
                {
                    EveryoneConfirmed = false;
                    break;
                }
            }

            return EveryoneConfirmed;
        }
    }

    public ActionTurn NextTurn()
    {
        if (CurrentState == ActionStates.New)
        {
            CurrentState = ActionStates.Pending;
            // Send Off All Actions
            LockstepManager.Instance.PendingTurn = this;
            LockstepManager.Instance.SendOffLocalActions();
        }
        else if (CurrentState == ActionStates.Pending)
        {
            // Recieved Everyones Actions
            LockstepManager.Instance.ConfirmedTurn = this;
            CurrentState = ActionStates.Confirmed;
        }
        else if (CurrentState == ActionStates.Confirmed)
        {
            CurrentState = ActionStates.Processing;
            // Process Everyone Turn
            LockstepManager.Instance.ProcessingTurn = this;
            LockstepManager.Instance.ProcessTurn();
        }
        else if (CurrentState == ActionStates.Processing)
        {
            CurrentState = ActionStates.Completed;
            // Move into Turn History
            CompletedOnGameTurn = LockstepManager.Instance.GameTurnCounter;
            LockstepManager.Instance.ActionTurnHistory.CompleteTurn(this);
        }

        return this;
    }

    public bool ContainsActionsFromAllPlayers()
    {
        foreach (NetworkPlayer player in PlayerManager.Instance.ConnectedPlayers.Values)
        {
            if (!ContainsActionFromPlayer(player.PlayerID))
            {
                return false;
            }
        }
        return true;
    }

    public bool ContainsActionFromPlayer(int PlayerID)
    {
        if (AllActionsDone == null)
        {
            AllActionsDone = new List<PlayerActions>();
            return false;
        }
        foreach (PlayerActions action in AllActionsDone)
        {
            if (action.PlayerID == PlayerID)
            {
                return true;
            }
        }

        return false;
    }

    public void AddActionSet(PlayerActions Action)
    {
        if (ContainsActionFromPlayer(Action.PlayerID))
        {
            if (Action.TurnNumber == LockStepTurnNumber)
            {
                Debug.LogWarning($"Tried to add a extra command set from player {Action.PlayerID}");
            }
            else
            {
                if (Action.TurnNumber < LockStepTurnNumber)
                {
                    Debug.LogWarning($"Tried to add a extra command set from player for a previous action set {Action.PlayerID}");
                }
                else
                {
                    if (this == LockstepManager.Instance.CurrentTurn)
                    {
                        Debug.LogWarning($"Tried to add a extra command set from player beyond next action set {Action.PlayerID} -> Action Turn {Action.TurnNumber} vs Current Turn {LockStepTurnNumber}");
                        return;
                    }
                    // Got the packets really fast, so set them to the next action set
                    LockstepManager.Instance.CurrentTurn.AddActionSet(Action);
                }
            }
        }
        else
        {
            AllActionsDone.Add(Action);
        }
    }

    public void Reconfirm()
    {
        foreach(int confirmedPlayer in ConfirmedPlayers)
        {
            if (confirmedPlayer == PlayerManager.Instance.LocalPlayer.PlayerID)
            {
                return;
            }
        }

        LockstepManager.Instance.SendConfirmation(LockStepTurnNumber);
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

    public PlayerActions(ushort playerID = 0, bool UpdateTurnCounter = false, bool AddNoAction = true)
    {
        TurnNumber = TurnCounter;
        if (UpdateTurnCounter)
        {
            TurnCounter++;
        }
        PlayerID = playerID;

        if (AddNoAction)
        {
            NoAction NewNoAction = new NoAction();
            NewNoAction.OwningPlayer = playerID;
            AddAction(NewNoAction);
        }
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
    BuyUnit = 2,
}

[System.Serializable]
public class NoAction : IAction
{
    public int ActionType { get; set; }
    public ushort OwningPlayer { get; set; }

    public NoAction()
    {
        ActionType = (int)ActionTypes.NoAction;

        if (PlayerManager.Instance.ActiveOnlineMode == PlayModes.CustomGame || PlayerManager.Instance.ActiveOnlineMode == PlayModes.Ranked || PlayerManager.Instance.ActiveOnlineMode == PlayModes.Quickplay)
        {
            OwningPlayer = PlayerManager.Instance.LocalPlayer.PlayerID;
        }
        else
        {
            OwningPlayer = 0;
        }
    }

    public void ProcessAction()
    {
        // Do Nothing
    }
}

public struct NoActionBroadcast : IBroadcast
{
    public int TurnNumber;
    public int NumberOfActions;
    public ushort SentByPlayer;
}

[System.Serializable]
public class CorruptAction : IAction
{
    public int ActionType { get; set; }
    public ushort OwningPlayer { get; set; }

    public CorruptAction()
    {
        ActionType = (int)ActionTypes.Corrupt;

        if (PlayerManager.Instance.ActiveOnlineMode == PlayModes.CustomGame || PlayerManager.Instance.ActiveOnlineMode == PlayModes.Ranked || PlayerManager.Instance.ActiveOnlineMode == PlayModes.Quickplay)
        {
            OwningPlayer = PlayerManager.Instance.LocalPlayer.PlayerID;
        }
        else
        {
            OwningPlayer = 0;
        }
    }

    public void ProcessAction()
    {
        // Something went wrong, post error
        Debug.LogError("Did not recieve action correctly, action was corrupted");
    }
}

public struct CorruptActionBroadcast : IBroadcast
{
    public int TurnNumber;
    public int NumberOfActions;
    public ushort SentByPlayer;
}

public interface IAction
{
    public int ActionType { get; set; }
    public ushort OwningPlayer { get; set; }

    public void ProcessAction();
}

public struct ActionConfirmationBroadcast : IBroadcast
{
    public int TurnNumber;
    public ushort SentByPlayer;
}

public struct CountdownBroadcast : IBroadcast
{
    public int ReconnectionSecond;
    public ushort SentByPlayer;
}

public struct PingBroadcast : IBroadcast
{
    public bool Reliable;
    public ushort SentByPlayer;
    public long ServerTimeNow;
}