using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Core.Interaction;
using UnityEngine;

namespace RPGame.Core.Tests
{
    public sealed class SelectionUtilityTests
    {
        private readonly List<GameObject> createdObjects = new();
        private GameObject interactorObject;
        private InteractionContext context;

        [SetUp]
        public void SetUp()
        {
            interactorObject = CreateObject("Interactor", Vector3.zero);
            context = new InteractionContext(interactorObject, interactorObject.transform);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < createdObjects.Count; i++)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
        }

        [Test]
        public void SelectBest_IgnoresInteractableBehindInteractor()
        {
            TestInteractable behind = CreateInteractable("Behind", new Vector3(0f, 0f, -1f));
            List<IInteractable> candidates = CreateCandidates(behind);

            IInteractable selected = SelectBest(candidates);

            Assert.IsNull(selected);
        }

        [Test]
        public void SelectBest_SelectsInteractableInFront()
        {
            TestInteractable inFront = CreateInteractable("InFront", new Vector3(0f, 0f, 1f));
            List<IInteractable> candidates = CreateCandidates(inFront);

            IInteractable selected = SelectBest(candidates);

            Assert.AreSame(inFront, selected);
        }

        [Test]
        public void SelectBest_PrefersBetterForwardScoreWhenDistancesAreSimilar()
        {
            TestInteractable centered = CreateInteractable("Centered", new Vector3(0f, 0f, 2f));
            TestInteractable angled = CreateInteractable("Angled", new Vector3(1.5f, 0f, 1.4f));
            List<IInteractable> candidates = CreateCandidates(centered, angled);

            IInteractable selected = SelectBest(candidates, forwardWeight: 4f, distanceWeight: 0.25f);

            Assert.AreSame(centered, selected);
        }

        [Test]
        public void SelectBest_PrefersCloserInteractableWhenAnglesAreSimilar()
        {
            TestInteractable close = CreateInteractable("Close", new Vector3(0f, 0f, 1f));
            TestInteractable far = CreateInteractable("Far", new Vector3(0f, 0f, 3f));
            List<IInteractable> candidates = CreateCandidates(far, close);

            IInteractable selected = SelectBest(candidates);

            Assert.AreSame(close, selected);
        }

        [Test]
        public void SelectBest_IgnoresInteractableThatCannotInteract()
        {
            TestInteractable blocked = CreateInteractable("Blocked", new Vector3(0f, 0f, 1f), canInteract: false);
            TestInteractable available = CreateInteractable("Available", new Vector3(0.5f, 0f, 2f));
            List<IInteractable> candidates = CreateCandidates(blocked, available);

            IInteractable selected = SelectBest(candidates);

            Assert.AreSame(available, selected);
        }

        private IInteractable SelectBest(
            IReadOnlyList<IInteractable> candidates,
            float minimumForwardDot = 0f,
            float forwardWeight = 2f,
            float distanceWeight = 1f)
        {
            return SelectionUtility.SelectBest(
                candidates,
                context,
                interactorObject.transform.position,
                Vector3.forward,
                minimumForwardDot,
                forwardWeight,
                distanceWeight);
        }

        private List<IInteractable> CreateCandidates(params TestInteractable[] interactables)
        {
            List<IInteractable> candidates = new();

            for (int i = 0; i < interactables.Length; i++)
            {
                candidates.Add(interactables[i]);
            }

            return candidates;
        }

        private TestInteractable CreateInteractable(string objectName, Vector3 position, bool canInteract = true)
        {
            GameObject gameObject = CreateObject(objectName, position);
            TestInteractable interactable = gameObject.AddComponent<TestInteractable>();
            interactable.CanInteractValue = canInteract;
            return interactable;
        }

        private GameObject CreateObject(string objectName, Vector3 position)
        {
            GameObject gameObject = new GameObject(objectName);
            gameObject.transform.position = position;
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private sealed class TestInteractable : MonoBehaviour, IInteractable
        {
            public bool CanInteractValue { get; set; } = true;
            public Transform InteractionTransform => transform;

            public bool CanInteract(InteractionContext context)
            {
                return CanInteractValue;
            }

            public void Interact(InteractionContext context)
            {
            }

            public string GetInteractionText()
            {
                return name;
            }
        }
    }
}
