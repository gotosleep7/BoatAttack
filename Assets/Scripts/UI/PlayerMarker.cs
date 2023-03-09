using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace BoatAttack.UI
{
    public class PlayerMarker : MonoBehaviour
    {
        public TextMeshProUGUI placeText;
        public TextMeshProUGUI nameText;

        private RectTransform _rect;
        private BoatData _boatData;
        private Boat _boat;
        private int _curPlace = -1;

        private void OnEnable()
        {
            RenderPipelineManager.beginFrameRendering += UpdatePosition;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginFrameRendering -= UpdatePosition;
        }

        public void Setup(BoatData boat)
        {
            _boatData = boat;
            _boat = boat.Boat;
            nameText.text = boat.boatName;
            _rect = transform as RectTransform;
        }

        private void LateUpdate()
        {
            UpdatePlace();
        }

        private void UpdatePlace()
        {
            if (!_boat || _curPlace == _boat.Place) return;
            _curPlace = _boat.Place;
            placeText.text = _curPlace.ToString();
        }

        private void UpdatePosition(ScriptableRenderContext context, Camera[] cameras)
        {
            Camera c = null;
            if (transform.parent.gameObject.layer == LayerMask.NameToLayer("Player1NameUI"))
            {
                c = RaceManager.Instance.player1Camera;
            }
            else if (transform.parent.gameObject.layer == LayerMask.NameToLayer("Player2NameUI"))
            {
                c = RaceManager.Instance.player2Camera;
            }

            if (_boatData == null || c == null) return; // if no boat or camera, the player marker cannot work
            var screenPos = c.WorldToViewportPoint(_boatData.BoatObject.transform.position + Vector3.up * 3f);
            if (screenPos.z < 0)
            {
                screenPos = -Vector3.one;
            }
            _rect.anchorMin = _rect.anchorMax = screenPos;
            _rect.anchoredPosition = Vector2.zero;
        }
    }
}
