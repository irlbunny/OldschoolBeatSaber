using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRUI
{
	public class VRUINavigationController : VRUIViewController
	{
		protected List<VRUIViewController> _viewControllers = new List<VRUIViewController>();

		public static VRUINavigationController CreateNavigationController(VRUIViewController rootViewController)
		{
			GameObject gameObject = new GameObject("VRUINavigationController", typeof(RectTransform));
			gameObject.SetActive(false);
			VRUINavigationController vRUINavigationController = gameObject.AddComponent<VRUINavigationController>();
			rootViewController.Init(vRUINavigationController.screen, null, vRUINavigationController);
			vRUINavigationController._viewControllers.Add(rootViewController);
			return vRUINavigationController;
		}

		public void InitWithRootViewController(VRUIViewController rootViewController)
		{
			rootViewController.Init(base.screen, null, this);
			_viewControllers.Add(rootViewController);
		}

		internal override void SetUserInteraction(bool enabled)
		{
			base.SetUserInteraction(enabled);
			int count = _viewControllers.Count;
			for (int i = 0; i < count; i++)
			{
				_viewControllers[i].SetUserInteraction(enabled);
			}
		}

		protected override void DidActivate()
		{
			base.DidActivate();
			LayoutViewControllers(_viewControllers);
		}

		protected override void DidDeactivate()
		{
			base.DidDeactivate();
		}

		internal override void Init(VRUIScreen screen, VRUIViewController parentViewController, VRUINavigationController navigationController)
		{
			base.Init(screen, parentViewController, navigationController);
			int count = _viewControllers.Count;
			for (int i = 0; i < count; i++)
			{
				_viewControllers[i].Init(screen, parentViewController, this);
			}
		}

		internal override void ResetViewController()
		{
			base.ResetViewController();
			int count = _viewControllers.Count;
			for (int i = 0; i < count; i++)
			{
				_viewControllers[i].ResetViewController();
			}
			_viewControllers.Clear();
		}

		internal override void Activate(bool beingPresented, bool movingToParentController)
		{
			base.Activate(beingPresented, movingToParentController);
			int count = _viewControllers.Count;
			for (int i = 0; i < count; i++)
			{
				_viewControllers[i].Activate(beingPresented, movingToParentController);
			}
		}

		internal override void Deactivate(bool beingDismissed, bool movingFromParentController, bool deactivateGameObject)
		{
			base.Deactivate(beingDismissed, movingFromParentController, deactivateGameObject);
			int count = _viewControllers.Count;
			for (int i = 0; i < count; i++)
			{
				_viewControllers[i].Deactivate(beingDismissed, movingFromParentController, deactivateGameObject);
			}
		}

		internal override void DeactivateGameObject()
		{
			base.DeactivateGameObject();
			int count = _viewControllers.Count;
			for (int i = 0; i < count; i++)
			{
				if (_viewControllers[i].gameObject.activeSelf)
				{
					_viewControllers[i].gameObject.SetActive(false);
				}
			}
		}

		public virtual void PushViewController(VRUIViewController viewController, bool immediately = false)
		{
			if (viewController.GetType().IsSubclassOf(typeof(VRUINavigationController)))
			{
				throw new InvalidOperationException("Can not push navigation controller into navigation controller hierarchy.");
			}
			if (_viewControllers.IndexOf(viewController) >= 0)
			{
				throw new InvalidOperationException("Can not push the same view controller into navigation controller hierarchy more than once.");
			}
			StopAllCoroutines();
			StartCoroutine(PushViewControllerCoroutine(viewController, immediately));
		}

		private IEnumerator PushViewControllerCoroutine(VRUIViewController viewController, bool immediately)
		{
			viewController.SetUserInteraction(false);
			_viewControllers.Add(viewController);
			viewController.Init(base.screen, base.parentViewController, this);
			float moveOffset = 4f / base.transform.lossyScale.x;
			viewController.transform.localPosition = new Vector3(moveOffset, 0f, 0f);
			viewController.Activate(false, true);
			float[] positions = GetNewXPositionsForViewControllers(_viewControllers);
			int numberOfViewControllers = _viewControllers.Count;
			if (!immediately)
			{
				float transitionDuration = 0.4f;
				float elapsedTime = 0f;
				float[] startXPositions = new float[numberOfViewControllers];
				for (int i = 0; i < numberOfViewControllers; i++)
				{
					startXPositions[i] = _viewControllers[i].transform.localPosition.x;
				}
				while (elapsedTime < transitionDuration)
				{
					float t = elapsedTime / transitionDuration;
					for (int j = 0; j < numberOfViewControllers; j++)
					{
						float x = Mathf.Lerp(startXPositions[j], positions[j], 1f - Mathf.Pow(1f - t, 3f));
						_viewControllers[j].transform.localPosition = new Vector3(x, 0f, 0f);
					}
					elapsedTime += Time.deltaTime;
					yield return null;
				}
			}
			for (int k = 0; k < numberOfViewControllers; k++)
			{
				_viewControllers[k].transform.localPosition = new Vector3(positions[k], 0f, 0f);
			}
			viewController.SetUserInteraction(true);
		}

		public virtual void PopViewController(bool immediately)
		{
		}

		private void LayoutViewControllers(List<VRUIViewController> viewControllers)
		{
			float[] newXPositionsForViewControllers = GetNewXPositionsForViewControllers(viewControllers);
			int count = viewControllers.Count;
			for (int i = 0; i < count; i++)
			{
				viewControllers[i].transform.localPosition = new Vector3(newXPositionsForViewControllers[i], 0f, 0f);
			}
		}

		private float[] GetNewXPositionsForViewControllers(List<VRUIViewController> viewControllers)
		{
			int count = viewControllers.Count;
			float[] array = new float[count];
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				RectTransform component = viewControllers[i].GetComponent<RectTransform>();
				num += component.rect.width * component.localScale.x;
			}
			float num2 = 0.05f;
			float num3 = (0f - (num + num2 * (float)(count - 1))) * 0.5f;
			for (int j = 0; j < count; j++)
			{
				RectTransform component2 = viewControllers[j].GetComponent<RectTransform>();
				array[j] = num3 + component2.rect.width * component2.localScale.x * 0.5f;
				num3 += component2.rect.width * component2.localScale.x + num2;
			}
			return array;
		}
	}
}
