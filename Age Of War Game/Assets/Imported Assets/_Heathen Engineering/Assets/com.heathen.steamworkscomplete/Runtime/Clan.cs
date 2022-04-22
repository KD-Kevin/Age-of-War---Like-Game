#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Steam Groups aka Steam Clans structure
    /// </summary>
    /// <remarks>
    /// This is interchangable with CSteamID and simply extends it with Clan related accessor features
    /// <para>
    /// You can fetch the list of available clans via API.Clans.Client.GetClans()
    /// </para>
    /// </remarks>
    [Serializable]
    public struct Clan : IEquatable<CSteamID>, IEquatable<Clan>, IEquatable<ulong>, IComparable<CSteamID>, IComparable<Clan>, IComparable<ulong>
    {
        public CSteamID id;
        public Texture2D Icon => API.Friends.Client.GetLoadedAvatar(id);
        public string Name => API.Clans.Client.GetName(id);
        public string Tag => API.Clans.Client.GetTag(id);
        public UserData Owner => API.Clans.Client.GetOwner(id);
        public UserData[] Officers => API.Clans.Client.GetOfficers(id);
        public int NumberOfMembersInChat => API.Clans.Client.GetChatMemberCount(id);
        public UserData[] MembersInChat => API.Clans.Client.GetChatMembers(id);
        public bool IsOfficalGameGroup => API.Clans.Client.IsClanOfficialGameGroup(this);
        public bool IsPublic => API.Clans.Client.IsClanPublic(this);
        public bool IsUserOwner => Owner == UserData.Me;
        public bool IsUserOfficer => Officers.Any(p => p == UserData.Me);
        /// <summary>
        /// Allows the user to join Steam group (clan) chats right within the game.
        /// </summary>
        /// <remarks>
        /// The joins the "old" group chat used in Steam not the new channel based chat system introduced 2018-2019.
        /// It is still usable but will not reflect the caht seen in the Steam client. 
        /// It can be used as a global chat for in-game players as connecting via the Steam API is the only known way to connect to this legacy chat system.
        /// </remarks>
        /// <param name="callback"></param>
        public void JoinChat(Action<ChatRoom, bool> callback) => API.Clans.Client.JoinChatRoom(id, callback);

        public void LoadIcon(Action<Texture2D> callback) => API.Friends.Client.GetFriendAvatar(id, callback);

        #region Boilerplate
        public int CompareTo(CSteamID other)
        {
            return id.CompareTo(other);
        }

        public int CompareTo(Clan other)
        {
            return id.CompareTo(other.id);
        }

        public int CompareTo(ulong other)
        {
            return id.m_SteamID.CompareTo(other);
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public bool Equals(Clan other)
        {
            return id.Equals(other.id);
        }

        public bool Equals(ulong other)
        {
            return id.m_SteamID.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return id.m_SteamID.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(Clan l, Clan r) => l.id == r.id;
        public static bool operator !=(Clan l, Clan r) => l.id != r.id;
        public static bool operator <(Clan l, Clan r) => l.id.m_SteamID < r.id.m_SteamID;
        public static bool operator >(Clan l, Clan r) => l.id.m_SteamID > r.id.m_SteamID;
        public static bool operator <=(Clan l, Clan r) => l.id.m_SteamID <= r.id.m_SteamID;
        public static bool operator >=(Clan l, Clan r) => l.id.m_SteamID >= r.id.m_SteamID;
        public static implicit operator CSteamID(Clan c) => c.id;
        public static implicit operator Clan(CSteamID id) => new Clan { id = id };
        public static implicit operator ulong(Clan c) => c.id.m_SteamID;
        public static implicit operator Clan(ulong id) => new Clan { id = new CSteamID(id) };
        #endregion
    }
}
#endif