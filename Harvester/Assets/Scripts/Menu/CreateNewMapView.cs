using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MasterServerToolkit.Bridges
{
    public class CreateNewMapView : UIView
    {
        [Header("Components"), SerializeField]
        private TMP_InputField MapNameInputField;

        protected override void Awake()
        {
            base.Awake();

            MapName = $"Map#{Mst.Helper.CreateFriendlyId()}";

            // Listen to show/hide events
            Mst.Events.AddListener(MstEventKeys.showCreateNewRoomView, OnShowMapNewRoomEventHandler);
            Mst.Events.AddListener(MstEventKeys.hideCreateNewRoomView, OnHideMapNewRoomEventHandler);
        }

        private void OnShowMapNewRoomEventHandler(EventMessage message)
        {
            Show();
        }

        private void OnHideMapNewRoomEventHandler(EventMessage message)
        {
            Hide();
        }

        public string MapName
        {
            get
            {
                return MapNameInputField != null ? MapNameInputField.text : string.Empty;
            }

            set
            {
                if (MapNameInputField)
                    MapNameInputField.text = value;
            }
        }

        public void CreateNewMap()
        {
            MapSaveData data = SaveManager.instance.LoadMapSaveData();
            data.maps.Add(new MapData(MapName));
            SaveManager.instance.SaveMapData(data);
        }
    }
}