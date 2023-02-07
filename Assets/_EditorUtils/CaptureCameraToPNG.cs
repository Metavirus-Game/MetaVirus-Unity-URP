using UnityEngine;

namespace _EditorUtils
{
    public enum CaptureMode
    {
        怪物整合模式3个一组,
        怪物独立模式
    }

    [RequireComponent(typeof(Camera))]
    public class CaptureCameraToPNG : MonoBehaviour
    {
        private Camera _camera;
        public string pngSavePath;

        public void CaptureScreenToPNG()
        {
        }
    }
}