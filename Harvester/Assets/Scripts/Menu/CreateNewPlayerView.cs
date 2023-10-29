using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;
using TMPro;
using UnityEngine;

namespace MasterServerToolkit.Bridges
{
    public class CreateNewPlayerView : UIView
    {
        [Header("Components"), SerializeField]
        private TMP_InputField playerNameInputField;

        protected override void Awake()
        {
            base.Awake();

            PlayerName = $"Player#{Mst.Helper.CreateFriendlyId()}";

            // Listen to show/hide events
            Mst.Events.AddListener(MstEventKeys.showCreateNewRoomView, OnShowCreateNewPlayerEventHandler);
            Mst.Events.AddListener(MstEventKeys.hideCreateNewRoomView, OnHideCreateNewPlayerEventHandler);
        }

        private void OnShowCreateNewPlayerEventHandler(EventMessage message)
        {
            Show();
        }

        private void OnHideCreateNewPlayerEventHandler(EventMessage message)
        {
            Hide();
        }

        public string PlayerName
        {
            get
            {
                return playerNameInputField != null ? playerNameInputField.text : string.Empty;
            }

            set
            {
                if (playerNameInputField)
                    playerNameInputField.text = value;
            }
        }


        public void CreateNewPlayer()
        {
            PlayerSaveData data = SaveManager.instance.LoadPlayerSaveData();
            data.players.Add(new PlayerData(PlayerName));
            SaveManager.instance.SavePlayerData(data);
        }
    }
}