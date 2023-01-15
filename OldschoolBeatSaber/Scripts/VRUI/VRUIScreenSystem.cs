using UnityEngine;

namespace VRUI
{
	public class VRUIScreenSystem : MonoBehaviour
	{
		[SerializeField]
		private VRUIScreen _mainScreen;

		[SerializeField]
		private VRUIScreen _leftScreen;

		[SerializeField]
		private VRUIScreen _rightScreen;

		public VRUIScreen mainScreen
		{
			get
			{
				return _mainScreen;
			}
		}

		public VRUIScreen leftScreen
		{
			get
			{
				return _leftScreen;
			}
		}

		public VRUIScreen rightScreen
		{
			get
			{
				return _rightScreen;
			}
		}

		private void Awake()
		{
			_mainScreen.screenSystem = this;
			_leftScreen.screenSystem = this;
			_rightScreen.screenSystem = this;
		}
	}
}
