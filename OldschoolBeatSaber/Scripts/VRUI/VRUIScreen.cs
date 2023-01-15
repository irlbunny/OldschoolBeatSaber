using UnityEngine;

namespace VRUI
{
	[RequireComponent(typeof(RectTransform))]
	public class VRUIScreen : MonoBehaviour
	{
		private VRUIViewController _rootViewController;

		public VRUIScreenSystem screenSystem { get; set; }

		public VRUIViewController rootViewController
		{
			get
			{
				return _rootViewController;
			}
		}

		public void SetRootViewController(VRUIViewController rootViewController)
		{
			if (rootViewController == _rootViewController)
			{
				return;
			}
			if (_rootViewController != null)
			{
				_rootViewController.Deactivate(true, false, true);
				_rootViewController.ResetViewController();
			}
			_rootViewController = rootViewController;
			if (_rootViewController != null)
			{
				Vector3 localScale = _rootViewController.transform.localScale;
				if (_rootViewController.transform.parent != base.transform)
				{
					_rootViewController.transform.SetParent(base.transform, false);
				}
				_rootViewController.transform.localScale = localScale;
				_rootViewController.transform.localPosition = Vector3.zero;
				Vector3 position = base.transform.position;
				_rootViewController.transform.rotation = Quaternion.LookRotation(base.transform.forward, Vector3.up);
				if (!_rootViewController.gameObject.activeSelf)
				{
					_rootViewController.gameObject.SetActive(true);
				}
				_rootViewController.Init(this, null, null);
				_rootViewController.Activate(true, false);
			}
		}
	}
}
