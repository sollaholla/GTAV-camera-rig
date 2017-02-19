#pragma warning disable 1587
/// 
/// Soloman Northrop © 2017
/// 
/// Please leave credit to the original author.
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using NativeUI;
using Control = GTA.Control;

namespace PhatCam
{
    public class MainScript : Script
    {
        // Menu stuff.
        private readonly Keys _menuKey;
        private readonly UIMenu _mainMenu;
        private readonly MenuPool _menuPool;

        // Cam stuff.
        private CameraController _cameraController;
        private float _xPos;
        private float _yPos;
        private float _zPos = 1.5f;
        private float _xPos2;
        private float _yPos2 = -3;
        private float _zPos2;
        private float _fov = 60;
        private float _sensitivity = 500;
        private bool _invertLook;
        private bool _disableInVehicle;
        private bool _firstPerson;

        // Player stuff.
        private float _speedBoost;

        public MainScript()
        {
            _mainMenu = new UIMenu("Phat Cam", "SELECT AN OPTION");
            _menuPool = new MenuPool();
            _menuPool.Add(_mainMenu);

            _menuKey = Settings.GetValue("menu", "menu_key", Keys.K);
            Settings.SetValue("menu", "menu_key", _menuKey);
            Settings.Save();

            AddItemsToMenu();

            Tick += OnTick;
            KeyUp += OnKeyUp;
            Aborted += OnAborted;
        }

        private void AddItemsToMenu()
        {
            List<dynamic> cache;
            var createCameraItem = new UIMenuItem("Create Camera");
            var removeCameraItem = new UIMenuItem("Remove Camera");
            var pivotOffsetMenu = _menuPool.AddSubMenu(_mainMenu, "Pivot Offset", "Set the camera's pivot offset.");
            var xPosItem = new CustomListItem("X Offset", cache = Enumerable.Range(-500, 1000).Select(i => (dynamic)i / 10f).ToList(), cache.IndexOf(_xPos));
            var yPosItem = new CustomListItem("Y Offset", cache = Enumerable.Range(-500, 1000).Select(i => (dynamic)i / 10f).ToList(), cache.IndexOf(_yPos));
            var zPosItem = new CustomListItem("Z Offset", cache = Enumerable.Range(-500, 1000).Select(i => (dynamic)i / 10f).ToList(), cache.IndexOf(_zPos));
            var cameraOffsetMenu = _menuPool.AddSubMenu(_mainMenu, "Camera Offset", "Set the camera's offset from the pivot.");
            var xPosItem2 = new CustomListItem("X Offset", cache = Enumerable.Range(-500, 1000).Select(i => (dynamic)i / 10f).ToList(), cache.IndexOf(_xPos2));
            var yPosItem2 = new CustomListItem("Y Offset", cache = Enumerable.Range(-500, 1000).Select(i => (dynamic)i / 10f).ToList(), cache.IndexOf(_yPos2));
            var zPosItem2 = new CustomListItem("Z Offset", cache = Enumerable.Range(-500, 1000).Select(i => (dynamic)i / 10f).ToList(), cache.IndexOf(_zPos2));
            var cameraFieldOfViewItem = new CustomListItem("Field Of View", cache = Enumerable.Range(1, 179).Select(i => (dynamic)i).ToList(), cache.IndexOf(60));
            var sensitivtyItem = new UIMenuItem("Sensitivity", "Set the sensitivity.");
            var invertLookItem = new UIMenuCheckboxItem("Invert Look", false, "Invert up and down look.");
            var speedBoostItem = new CustomListItem("Player Speed Boost",
                cache = Enumerable.Range(0, 50).Select(i => (dynamic) (float) i).ToList(), cache.IndexOf(0f),
                "Used so larger characters " +
                "can walk and move faster since root motion keeps them at the same speed as a regular sized characters.");
            var disableInVehicleItem = new UIMenuCheckboxItem("Disable Cam In Vehicle", _disableInVehicle, "Disable the camera while in a vehicle.");

            sensitivtyItem.SetRightLabel(cameraFieldOfViewItem.Index.ToString());

            createCameraItem.Activated += CreateCameraItemOnActivated;
            removeCameraItem.Activated += RemoveCameraItemOnActivated;
            xPosItem.OnListChanged += XPosItemOnOnListChanged;
            yPosItem.OnListChanged += YPosItemOnOnListChanged;
            zPosItem.OnListChanged += ZPosItemOnOnListChanged;
            xPosItem2.OnListChanged += XPosItemOnOnListChanged2;
            yPosItem2.OnListChanged += YPosItemOnOnListChanged2;
            zPosItem2.OnListChanged += ZPosItemOnOnListChanged2;
            cameraFieldOfViewItem.OnListChanged += CameraFieldOfViewItemOnOnListChanged;
            sensitivtyItem.Activated += (sender, item) => GetUserInput(item, 3, ref _sensitivity);
            invertLookItem.CheckboxEvent += InvertLookItemOnCheckboxEvent;
            speedBoostItem.OnListChanged += SpeedBoostItemOnOnListChanged;
            disableInVehicleItem.CheckboxEvent += DisableInVehicleItemOnCheckboxEvent;

            xPosItem.Activated += (sender, item) => GetUserInput(item, 10, ref _xPos, xPosItem.IndexToItem(xPosItem.Index).ToString());
            yPosItem.Activated += (sender, item) => GetUserInput(item, 10, ref _yPos, yPosItem.IndexToItem(yPosItem.Index).ToString());
            zPosItem.Activated += (sender, item) => GetUserInput(item, 10, ref _zPos, zPosItem.IndexToItem(zPosItem.Index).ToString());
            xPosItem2.Activated += (sender, item) => GetUserInput(item, 10, ref _xPos2, xPosItem2.IndexToItem(xPosItem2.Index).ToString());
            yPosItem2.Activated += (sender, item) => GetUserInput(item, 10, ref _yPos2, yPosItem2.IndexToItem(yPosItem2.Index).ToString());
            zPosItem2.Activated += (sender, item) => GetUserInput(item, 10, ref _zPos2, zPosItem2.IndexToItem(zPosItem2.Index).ToString());
            speedBoostItem.Activated += (sender, item) => GetUserInput(item, 2, ref _speedBoost, speedBoostItem.IndexToItem(speedBoostItem.Index).ToString());

            pivotOffsetMenu.AddItem(xPosItem);
            pivotOffsetMenu.AddItem(yPosItem);
            pivotOffsetMenu.AddItem(zPosItem);
            cameraOffsetMenu.AddItem(xPosItem2);
            cameraOffsetMenu.AddItem(yPosItem2);
            cameraOffsetMenu.AddItem(zPosItem2);
            _mainMenu.AddItem(cameraFieldOfViewItem);
            _mainMenu.AddItem(invertLookItem);
            _mainMenu.AddItem(disableInVehicleItem);
            _mainMenu.AddItem(createCameraItem);
            _mainMenu.AddItem(removeCameraItem);
            _mainMenu.AddItem(speedBoostItem);
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            _menuPool.ProcessMenus();

            var playerCharacter = Game.Player.Character;
            var speed = _speedBoost;
            if (_speedBoost > 1 && !playerCharacter.IsRagdoll && !playerCharacter.IsInAir)
            {
                var ray = World.Raycast(playerCharacter.Position, Vector3.WorldDown, 5, IntersectOptions.Everything,
                    playerCharacter);
                if (ray.DitHitAnything)
                {
                    var normal = ray.SurfaceNormal;
                    var quaternion = Quaternion.FromToRotation(Vector3.WorldUp, normal) * playerCharacter.Quaternion;
                    var direction = quaternion * Vector3.RelativeFront;

                    // 100% (or 100) / 3 = 33.3 meaning we will increment by 0.33 
                    // to acheive an ordered speed for [walk > run > sprint]
                    if (playerCharacter.IsWalking)
                        playerCharacter.Velocity = direction * speed * 0.33f;
                    else if (playerCharacter.IsRunning)
                        playerCharacter.Velocity = direction * speed * 0.66f;
                    else if (playerCharacter.IsSprinting)
                        playerCharacter.Velocity = direction * speed;
                }
            }

            if (_cameraController == null) return;

            _cameraController.Rendering = 
                FollowCam.ViewMode != FollowCamViewMode.FirstPerson &&
                !(Game.Player.IsAiming && playerCharacter.Weapons.Current.Group == WeaponGroup.Sniper) &&
                !(playerCharacter.IsInVehicle() && _disableInVehicle);

            _cameraController.Update(_invertLook, -60, 70, _sensitivity);
            _cameraController.PivotOffset = new Vector3(_xPos, _yPos, _zPos);
            _cameraController.CameraOffset = new Vector3(_xPos2, _yPos2, _zPos2);
            _cameraController.FieldOfView = _fov - (Game.Player.IsAiming ? 10 : 0);

            ZoomCameraViewMode();
        }

        private void ZoomCameraViewMode()
        {
            Game.DisableControlThisFrame(2, Control.NextCamera);
            if (Game.IsDisabledControlJustPressed(2, Control.NextCamera))
                _firstPerson = !_firstPerson;
            FollowCam.ViewMode = _firstPerson ? FollowCamViewMode.FirstPerson : FollowCamViewMode.ThirdPersonNear;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_menuPool.IsAnyMenuOpen()) return;
            if (e.KeyCode != _menuKey) return;
            _mainMenu.Visible = !_mainMenu.Visible;
        }

        private void OnAborted(object sender, EventArgs eventArgs) => _cameraController?.Delete();

        private void CreateCameraItemOnActivated(UIMenu sender, UIMenuItem selectedItem)
        {
            if (_cameraController != null) return;
            _cameraController = new CameraController(new Vector3(_xPos, _yPos, _zPos), new Vector3(_xPos2, _yPos2, _zPos2), _fov);
        }

        private void RemoveCameraItemOnActivated(UIMenu sender, UIMenuItem selectedItem)
        {
            _cameraController.Delete();
            _cameraController = null;
        }

        private void XPosItemOnOnListChanged(UIMenuListItem sender, int newIndex) => _xPos = (float)sender.IndexToItem(newIndex);
        private void YPosItemOnOnListChanged(UIMenuListItem sender, int newIndex) => _yPos = (float)sender.IndexToItem(newIndex);
        private void ZPosItemOnOnListChanged(UIMenuListItem sender, int newIndex) => _zPos = (float)sender.IndexToItem(newIndex);
        private void CameraFieldOfViewItemOnOnListChanged(UIMenuListItem sender, int newIndex) => _fov = newIndex;
        private void XPosItemOnOnListChanged2(UIMenuListItem sender, int newindex) => _xPos2 = (float)sender.IndexToItem(newindex);
        private void YPosItemOnOnListChanged2(UIMenuListItem sender, int newindex) => _yPos2 = (float)sender.IndexToItem(newindex);
        private void ZPosItemOnOnListChanged2(UIMenuListItem sender, int newindex) => _zPos2 = (float)sender.IndexToItem(newindex);
        private void SpeedBoostItemOnOnListChanged(UIMenuListItem sender, int newIndex) => _speedBoost = newIndex;
        private void DisableInVehicleItemOnCheckboxEvent(UIMenuCheckboxItem sender, bool @checked) => _disableInVehicle = @checked;
        private void InvertLookItemOnCheckboxEvent(UIMenuCheckboxItem sender, bool @checked) => _invertLook = @checked;

        #pragma warning disable 1574
        /// <summary>
        /// Get's input from the <see cref="Game.GetUserInput"/> method, and sets the returnValue to a parsed float.
        /// </summary>
        /// <param name="selectedItem">The menu item. If this is a list item, we will try to set the value of the index. If this 
        /// is a regular <see cref="UIMenuItem"/> we will set the right badge of the item.</param>
        /// <param name="maxLetters">The maximum amount of letters the input box can recieve.</param>
        /// <param name="returnValue">The value from the user's input that will be returned as a <see cref="float"/>.</param>
        /// <param name="defaultText">The default text of the input box.</param>
        public void GetUserInput(UIMenuItem selectedItem, int maxLetters, ref float returnValue, string defaultText = "")
        {
            var input = Game.GetUserInput(defaultText, maxLetters);

            float value;

            if (!float.TryParse(input, out value))
                return;

            returnValue = value;

            if (selectedItem.GetType() == typeof(CustomListItem))
            {
                var uiListItem = selectedItem as CustomListItem;
                try {
                    if (uiListItem != null) uiListItem.Index = uiListItem.ItemToIndex(value);
                }
                catch {
                    UI.ShowSubtitle("Failed to set value.");
                }
                return;
            }
            selectedItem.SetRightLabel($"{returnValue}");
        }
    }
}
