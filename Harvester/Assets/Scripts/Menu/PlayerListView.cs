using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MasterServerToolkit.Bridges
{
    public class PlayerListView : UIView
    {
        [Header("Components"), SerializeField]
        private UILable uiLablePrefab;
        [SerializeField]
        private UILable uiColLablePrefab;
        [SerializeField]
        private UIButton uiButtonPrefab;
        [SerializeField]
        private RectTransform listContainer;
        [SerializeField]
        private TMP_Text statusInfoText;

        public PickedData pickedData;

        public GamesListView gamesListView;

        protected override void Awake()
        {
            base.Awake();

            // Listen to show/hide events
            Mst.Events.AddListener(MstEventKeys.showGamesListView, OnShowPlayerListEventHandler);
            Mst.Events.AddListener(MstEventKeys.hideGamesListView, OnHidePlayerListEventHandler);
        }

        protected override void Start()
        {
            base.Start();

            if (listContainer)
            {
                foreach (Transform t in listContainer)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private void OnShowPlayerListEventHandler(EventMessage message)
        {
            Show();
        }

        private void OnHidePlayerListEventHandler(EventMessage message)
        {
            Hide();
        }

        protected override void OnShow()
        {
            FindPlayers();
        }

        private void DrawPlayersList(PlayerSaveData playerData)
        {
            if (listContainer)
            {
                int index = 0;

                var gameNumberCol = Instantiate(uiColLablePrefab, listContainer, false);
                gameNumberCol.Text = "#";
                gameNumberCol.name = "gameNumberCol";

                var gameNameCol = Instantiate(uiColLablePrefab, listContainer, false);
                gameNameCol.Text = "Name";
                gameNameCol.name = "gameNameCol";

                var connectBtnCol = Instantiate(uiColLablePrefab, listContainer, false);
                connectBtnCol.Text = "#";
                connectBtnCol.name = "connectBtnCol";

                foreach (PlayerData player in playerData.players)
                {
                    var gameNumberLable = Instantiate(uiLablePrefab, listContainer, false);
                    gameNumberLable.Text = $"{index + 1}";
                    gameNumberLable.name = $"gameNumberLable_{index}";

                    var gameNameLable = Instantiate(uiLablePrefab, listContainer, false);
                    gameNameLable.Text = player.playerName.ToString();
                    gameNameLable.name = $"gameNameLable_{index}";

                    var gameConnectBtn = Instantiate(uiButtonPrefab, listContainer, false);
                    gameConnectBtn.SetLable("Select");
                    gameConnectBtn.SetID(index);
                    gameConnectBtn.AddOnClickListener(() =>
                    {
                        pickedData.playerID = gameConnectBtn.GetID();
                        gamesListView.Show();
                        Hide();
                    });
                    gameConnectBtn.name = $"gameConnectBtn_{index}";

                    index++;
                }
            }
            else
            {
                logger.Error("Not all components are setup");
            }
        }

        private void ClearPlayersList()
        {
            if (listContainer)
            {
                foreach (Transform tr in listContainer)
                {
                    Destroy(tr.gameObject);
                }
            }
        }

        public void FindPlayers()
        {
            ClearPlayersList();
            if (statusInfoText)
            {
                statusInfoText.text = "Finding players... Please wait!";
                statusInfoText.gameObject.SetActive(true);
            }
            PlayerSaveData data = SaveManager.instance.LoadPlayerSaveData();
            if (data.players.Count == 0)
            {
                statusInfoText.text = "No players found! Try to create one.";
                return;
            }
            statusInfoText.gameObject.SetActive(false);
            DrawPlayersList(data);
        }
    }
}