using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VRUI
{
	[RequireComponent(typeof(RectTransform))]
	public class VRUIViewController : MonoBehaviour
	{
		private VRUINavigationController _navigationController;

		private VRUIViewController _parentViewController;

		private VRUIViewController _childViewController;

		private VRUIScreen _screen;

		private BaseRaycaster _raycaster;

		private RectTransform _rectTransform;

		public VRUINavigationController navigationController
		{
			get
			{
				return _navigationController;
			}
		}

		public VRUIScreen screen
		{
			get
			{
				return _screen;
			}
		}

		public VRUIViewController parentViewController
		{
			get
			{
				return _parentViewController;
			}
		}

		public VRUIViewController childViewController
		{
			get
			{
				return _childViewController;
			}
		}

		public RectTransform rectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public bool controllerActive { get; private set; }

		public bool beingPresented { get; private set; }

		public bool beingDismissed { get; private set; }

		public bool movingToParentController { get; private set; }

		public bool movingFromParentController { get; private set; }

		protected virtual void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
			_raycaster = GetComponent<BaseRaycaster>();
		}

		internal virtual void SetUserInteraction(bool enabled)
		{
			if (_raycaster != null)
			{
				_raycaster.enabled = enabled;
			}
		}

		protected virtual void DidActivate()
		{
		}

		protected virtual void DidDeactivate()
		{
		}

		internal virtual void Init(VRUIScreen screen, VRUIViewController parentViewController, VRUINavigationController navigationController)
		{
			_screen = screen;
			_parentViewController = parentViewController;
			_navigationController = navigationController;
			if (_navigationController != null)
			{
				base.transform.SetParent(navigationController.transform, false);
				base.transform.SetSiblingIndex(0);
			}
			else
			{
				base.transform.SetParent(screen.transform, false);
			}
		}

		internal virtual void ResetViewController()
		{
			base.transform.SetParent(_screen.transform, false);
			_screen = null;
			_parentViewController = null;
			_navigationController = null;
			_childViewController = null;
		}

		public void PresentModalViewController(VRUIViewController viewController, Action finishedCallback, bool immediately = false)
		{
			if (_childViewController != null)
			{
				throw new InvalidOperationException("Can not present new view controller. This view controller is already presenting one.");
			}
			StartCoroutine(PresentModalViewControllerCoroutine(viewController, finishedCallback, immediately));
		}

		private IEnumerator PresentModalViewControllerCoroutine(VRUIViewController viewController, Action finishedCallback, bool immediately)
		{
			EventSystem eventSystem = EventSystem.current;
			eventSystem.SetSelectedGameObject(null);
			SetUserInteraction(false);
			float transitionDuration = 0.4f;
			float elapsedTime = 0f;
			Deactivate(false, false, false);
			if (!viewController.gameObject.activeSelf)
			{
				viewController.gameObject.SetActive(true);
			}
			viewController.Init(_screen, this, null);
			viewController.Activate(true, false);
			viewController.SetUserInteraction(false);
			_childViewController = viewController;
			float moveOffset = 20f / base.transform.lossyScale.x;
			if (!immediately)
			{
				while (elapsedTime < transitionDuration)
				{
					float t = elapsedTime / transitionDuration;
					viewController.transform.localPosition = Vector3.Lerp(new Vector3(moveOffset, 0f, 0f), Vector3.zero, 1f - Mathf.Pow(1f - t, 3f));
					base.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0f - moveOffset, 0f, 0f), Mathf.Pow(t, 3f));
					elapsedTime += Time.deltaTime;
					yield return null;
				}
			}
			viewController.transform.localPosition = Vector3.zero;
			base.transform.localPosition = new Vector3(0f - moveOffset, 0f, 0f);
			base.gameObject.SetActive(false);
			viewController.SetUserInteraction(true);
			if (finishedCallback != null)
			{
				finishedCallback();
			}
		}

		public void DismissModalViewController(Action finishedCallback, bool immediately = false)
		{
			if (_parentViewController == null)
			{
				throw new InvalidOperationException("This view controller can not be dismissed, because it does not have any parent.");
			}
			StartCoroutine(DismissModalViewControllerCoroutine(finishedCallback, immediately));
		}

		private IEnumerator DismissModalViewControllerCoroutine(Action finishedCallback, bool immediately)
		{
			EventSystem.current.SetSelectedGameObject(null);
			VRUIViewController movingInViewController = _parentViewController;
			Transform movingOutObjectTransform = base.transform;
			SetUserInteraction(false);
			Deactivate(true, false, false);
			movingInViewController._childViewController = null;
			movingInViewController.Activate(false, false);
			Transform movingInObjectTransform = movingInViewController.transform;
			float moveOffset = 20f / base.transform.lossyScale.x;
			if (!immediately)
			{
				float transitionDuration = 0.4f;
				float elapsedTime = 0f;
				while (elapsedTime < transitionDuration)
				{
					float t = elapsedTime / transitionDuration;
					movingInObjectTransform.localPosition = Vector3.Lerp(new Vector3(0f - moveOffset, 0f, 0f), Vector3.zero, 1f - Mathf.Pow(1f - t, 3f));
					movingOutObjectTransform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(moveOffset, 0f, 0f), Mathf.Pow(t, 3f));
					elapsedTime += Time.deltaTime;
					yield return null;
				}
			}
			movingInObjectTransform.localPosition = Vector3.zero;
			movingOutObjectTransform.localPosition = new Vector3(moveOffset, 0f, 0f);
			DeactivateGameObject();
			ResetViewController();
			movingInViewController.SetUserInteraction(true);
			if (finishedCallback != null)
			{
				finishedCallback();
			}
		}

		internal virtual void Activate(bool beingPresented, bool movingToParentController)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
			controllerActive = true;
			this.beingPresented = beingPresented;
			this.movingToParentController = movingToParentController;
			DidActivate();
		}

		internal virtual void Deactivate(bool beingDismissed, bool movingFromParentController, bool deactivateGameObject)
		{
			if (base.gameObject.activeSelf && deactivateGameObject)
			{
				base.gameObject.SetActive(false);
			}
			controllerActive = false;
			this.beingDismissed = beingDismissed;
			this.movingFromParentController = movingFromParentController;
			DidDeactivate();
		}

		internal virtual void DeactivateGameObject()
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
		}
	}
}
