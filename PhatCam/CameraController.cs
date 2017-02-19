using System.Drawing;
using GTA;
using GTA.Math;
using NativeUI;

namespace PhatCam
{
    public class CameraController
    {
        private float _x;

        private readonly Entity _pivot;
        private readonly Camera _camera;
        private readonly UIResText _artificalCrosshair;

        public CameraController(Vector3 pivotOffset, Vector3 cameraOffset, float fieldOfView)
        {
            _pivot = World.CreateProp("prop_cs_dildo_01", Game.Player.Character.Position + pivotOffset, GameplayCamera.Rotation, false, false);
            _pivot.HasCollision = false;
            _pivot.IsPersistent = true;
            _pivot.IsVisible = false;
            _camera = World.CreateCamera(Game.Player.Character.Position, GameplayCamera.Rotation, fieldOfView);
            _camera.AttachTo(_pivot, cameraOffset);
            PivotOffset = pivotOffset;
            _artificalCrosshair = new UIResText("+", new Point(), 0.3f);
        }

        public bool Rendering {
            get { return World.RenderingCamera == _camera; }
            set { World.RenderingCamera = value ? _camera : null; }
        }

        public Vector3 PivotOffset { get; set; }

        public Vector3 CameraOffset
        {
            set { _camera.AttachTo(_pivot, value); }
        }

        public float FieldOfView
        {
            get { return _camera.FieldOfView; }
            set { _camera.FieldOfView = value; }
        }

        public void Update(bool invert, float minX, float maxX, float sensitivity)
        {
            if ((Game.Player.IsAiming || Game.Player.Character.IsShooting) && Rendering)
            {
                _artificalCrosshair.Position = new Point((int) (UIMenu.GetScreenResolutionMantainRatio().Width / 2) - 5,
                    (int) (UIMenu.GetScreenResolutionMantainRatio().Height / 2) - 15);
                _artificalCrosshair.Draw();
            }

            if (Game.Player.Character.CurrentVehicle != null)
                _pivot.SetNoCollision(Game.Player.Character.CurrentVehicle, true);

            // Move with the target.
            _pivot.Position = Game.Player.Character.Position + Game.Player.Character.Quaternion * PivotOffset;

            // Get the mouse x input axes.
            var xInput = Game.GetControlNormal(2, Control.LookUpDown);

            // NOTE: You could even go as far as to implement your own z axis rotation, as shown for the xInput axis.

            // Use the default sensitivity.
            _x -= xInput * sensitivity * Game.LastFrameTime;
            
            // Invert if we choose.
            if (invert)
            {
                _x = -_x;
            }

            // Clamp the x axis.
            _x = Mathf.Clamp(_x, minX, maxX);

            // Get the rotation.
            var rotation = new Vector3(_x, 0, GameplayCamera.Rotation.Z);

            // Rotate the pivot.
            _pivot.Rotation = rotation;

            _camera.Rotation = _pivot.Rotation;
        }

        /// <summary>
        /// Delete this camera.
        /// </summary>
        public void Delete()
        {
            World.RenderingCamera = null;
            _camera.Destroy();
            _pivot.Delete();
        }
    }
}
