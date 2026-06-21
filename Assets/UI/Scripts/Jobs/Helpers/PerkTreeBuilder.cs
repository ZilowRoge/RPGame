using System;
using System.Collections.Generic;
using RPGame.Progression;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RPGame.UI.Jobs
{
    public static class PerkTreeBuilder
    {
        public static int Rebuild(
            JobInstance job,
            RectTransform perkNodesParent,
            PerkTreeConnectionsGraphic connectionsGraphic,
            RectTransform rootNodePrefab,
            RectTransform perkNodePrefab,
            List<GameObject> spawnedPerkNodes,
            Func<PerkDefinition, PerkUnlockState> getUnlockState,
            Func<PerkDefinition, bool> isPerkPending,
            Action<PerkDefinition> togglePendingPerk,
            Action<PerkDefinition, Vector2> showTooltip,
            Action hideTooltip,
            Action refreshRequested)
        {
            Clear(spawnedPerkNodes, perkNodesParent);

            if (job?.Definition == null || perkNodesParent == null || spawnedPerkNodes == null)
            {
                if (perkNodesParent != null)
                {
                    GetConnectionsGraphic(perkNodesParent, connectionsGraphic)?.SetConnections(null);
                }

                return 0;
            }

            List<PerkDefinition> spawnedPerks = new();
            List<PerkTreeConnection> connections = new();
            GetReachablePerksAndConnections(
                job.Definition.JobPerks,
                spawnedPerks,
                connections,
                getUnlockState,
                isPerkPending);
            GetConnectionsGraphic(perkNodesParent, connectionsGraphic)?.SetConnections(connections);

            SpawnRootNode(rootNodePrefab, perkNodesParent, spawnedPerkNodes);
            SpawnPerkNodes(
                spawnedPerks,
                perkNodesParent,
                perkNodePrefab,
                spawnedPerkNodes,
                getUnlockState,
                isPerkPending,
                togglePendingPerk,
                showTooltip,
                hideTooltip,
                refreshRequested);

            return spawnedPerkNodes.Count;
        }

        private static void SpawnRootNode(
            RectTransform rootNodePrefab,
            RectTransform perkNodesParent,
            List<GameObject> spawnedPerkNodes)
        {
            if (rootNodePrefab == null)
            {
                return;
            }

            RectTransform rootNode = UnityEngine.Object.Instantiate(rootNodePrefab, perkNodesParent);
            rootNode.name = rootNodePrefab.name;
            rootNode.anchoredPosition = Vector2.zero;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(rootNode.gameObject, "Rebuild Perk Tree");
            }
#endif
            spawnedPerkNodes.Add(rootNode.gameObject);
        }

        private static void GetReachablePerksAndConnections(
            IReadOnlyList<PerkDefinition> jobPerks,
            List<PerkDefinition> spawnedPerks,
            List<PerkTreeConnection> connections,
            Func<PerkDefinition, PerkUnlockState> getUnlockState,
            Func<PerkDefinition, bool> isPerkPending)
        {
            Dictionary<string, PerkDefinition> perksById = new();
            Queue<PerkDefinition> perksToSpawn = new();
            HashSet<string> spawnedPerkIds = new();
            HashSet<string> connectionKeys = new();

            for (int i = 0; i < jobPerks.Count; i++)
            {
                PerkDefinition perk = jobPerks[i];
                if (perk == null || string.IsNullOrWhiteSpace(perk.PerkId))
                {
                    continue;
                }

                perksById.TryAdd(perk.PerkId, perk);

                if (perk.IsStartingPerk)
                {
                    perksToSpawn.Enqueue(perk);
                    AddConnection(
                        connections,
                        connectionKeys,
                        string.Empty,
                        perk.PerkId,
                        Vector2.zero,
                        perk.UIPosition,
                        GetConnectionState(perk, null, getUnlockState, isPerkPending));
                }
            }

            while (perksToSpawn.Count > 0)
            {
                PerkDefinition perk = perksToSpawn.Dequeue();
                if (perk == null || !spawnedPerkIds.Add(perk.PerkId))
                {
                    continue;
                }

                spawnedPerks.Add(perk);

                foreach (PerkDefinition connectedPerk in GetConnectedPerks(perk, perksById.Values))
                {
                    AddConnection(
                        connections,
                        connectionKeys,
                        perk.PerkId,
                        connectedPerk.PerkId,
                        perk.UIPosition,
                        connectedPerk.UIPosition,
                        GetConnectionState(perk, connectedPerk, getUnlockState, isPerkPending));

                    if (!spawnedPerkIds.Contains(connectedPerk.PerkId))
                    {
                        perksToSpawn.Enqueue(connectedPerk);
                    }
                }
            }
        }

        private static IEnumerable<PerkDefinition> GetConnectedPerks(
            PerkDefinition perk,
            IEnumerable<PerkDefinition> jobPerks)
        {
            foreach (PerkDefinition connectedPerk in jobPerks)
            {
                if (connectedPerk != null && connectedPerk.PerkId != perk.PerkId && AreConnected(perk, connectedPerk))
                {
                    yield return connectedPerk;
                }
            }
        }

        private static bool AreConnected(PerkDefinition firstPerk, PerkDefinition secondPerk)
        {
            return ContainsPerkId(firstPerk.ConnectedPerkIds, secondPerk.PerkId)
                || ContainsPerkId(secondPerk.ConnectedPerkIds, firstPerk.PerkId);
        }

        private static bool ContainsPerkId(IReadOnlyList<string> perkIds, string perkId)
        {
            for (int i = 0; i < perkIds.Count; i++)
            {
                if (perkIds[i] == perkId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SpawnPerkNodes(
            IReadOnlyList<PerkDefinition> perks,
            RectTransform perkNodesParent,
            RectTransform perkNodePrefab,
            List<GameObject> spawnedPerkNodes,
            Func<PerkDefinition, PerkUnlockState> getUnlockState,
            Func<PerkDefinition, bool> isPerkPending,
            Action<PerkDefinition> togglePendingPerk,
            Action<PerkDefinition, Vector2> showTooltip,
            Action hideTooltip,
            Action refreshRequested)
        {
            for (int i = 0; i < perks.Count; i++)
            {
                SpawnPerkNode(
                    perks[i],
                    perkNodesParent,
                    perkNodePrefab,
                    spawnedPerkNodes,
                    getUnlockState,
                    isPerkPending,
                    togglePendingPerk,
                    showTooltip,
                    hideTooltip,
                    refreshRequested);
            }
        }

        private static void AddConnection(
            List<PerkTreeConnection> connections,
            HashSet<string> connectionKeys,
            string fromPerkId,
            string toPerkId,
            Vector2 fromPosition,
            Vector2 toPosition,
            PerkTreeConnectionState state)
        {
            string connectionKey = GetConnectionKey(fromPerkId, toPerkId);
            if (connectionKeys.Add(connectionKey))
            {
                connections.Add(new PerkTreeConnection(fromPosition, toPosition, state));
            }
        }

        private static PerkTreeConnectionState GetConnectionState(
            PerkDefinition firstPerk,
            PerkDefinition secondPerk,
            Func<PerkDefinition, PerkUnlockState> getUnlockState,
            Func<PerkDefinition, bool> isPerkPending)
        {
            PerkUnlockState firstState = getUnlockState != null ? getUnlockState(firstPerk) : PerkUnlockState.Locked;
            PerkUnlockState secondState = secondPerk != null && getUnlockState != null
                ? getUnlockState(secondPerk)
                : firstState;
            bool firstPending = isPerkPending != null && isPerkPending(firstPerk);
            bool secondPending = secondPerk != null && isPerkPending != null && isPerkPending(secondPerk);

            if (firstPending || secondPending)
            {
                return PerkTreeConnectionState.Pending;
            }

            if (firstState == PerkUnlockState.Unlocked && (secondPerk == null || secondState == PerkUnlockState.Unlocked))
            {
                return PerkTreeConnectionState.Unlocked;
            }

            if (firstState == PerkUnlockState.Available || secondState == PerkUnlockState.Available)
            {
                return PerkTreeConnectionState.Available;
            }

            return PerkTreeConnectionState.Locked;
        }

        private static string GetConnectionKey(string fromPerkId, string toPerkId)
        {
            return string.CompareOrdinal(fromPerkId, toPerkId) <= 0
                ? $"{fromPerkId}:{toPerkId}"
                : $"{toPerkId}:{fromPerkId}";
        }

        private static PerkTreeConnectionsGraphic GetConnectionsGraphic(
            RectTransform perkNodesParent,
            PerkTreeConnectionsGraphic connectionsGraphic)
        {
            return connectionsGraphic != null
                ? connectionsGraphic
                : perkNodesParent.GetComponent<PerkTreeConnectionsGraphic>();
        }

        private static void Clear(List<GameObject> spawnedPerkNodes, RectTransform perkNodesParent)
        {
            if (spawnedPerkNodes == null)
            {
                return;
            }

            for (int i = spawnedPerkNodes.Count - 1; i >= 0; i--)
            {
                if (spawnedPerkNodes[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(spawnedPerkNodes[i]);
                    }
                    else
                    {
#if UNITY_EDITOR
                        Undo.DestroyObjectImmediate(spawnedPerkNodes[i]);
#else
                        UnityEngine.Object.DestroyImmediate(spawnedPerkNodes[i]);
#endif
                    }
                }
            }

            spawnedPerkNodes.Clear();

            if (Application.isPlaying || perkNodesParent == null)
            {
                return;
            }

            for (int i = perkNodesParent.childCount - 1; i >= 0; i--)
            {
                Transform child = perkNodesParent.GetChild(i);
                if (child != null)
                {
#if UNITY_EDITOR
                    Undo.DestroyObjectImmediate(child.gameObject);
#else
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
#endif
                }
            }
        }

        private static void SpawnPerkNode(
            PerkDefinition perk,
            RectTransform perkNodesParent,
            RectTransform perkNodePrefab,
            List<GameObject> spawnedPerkNodes,
            Func<PerkDefinition, PerkUnlockState> getUnlockState,
            Func<PerkDefinition, bool> isPerkPending,
            Action<PerkDefinition> togglePendingPerk,
            Action<PerkDefinition, Vector2> showTooltip,
            Action hideTooltip,
            Action refreshRequested)
        {
            if (perk == null || perkNodePrefab == null)
            {
                return;
            }

            RectTransform node = UnityEngine.Object.Instantiate(perkNodePrefab, perkNodesParent);
            node.name = string.IsNullOrWhiteSpace(perk.PerkId) ? perkNodePrefab.name : perk.PerkId;
            node.anchoredPosition = perk.UIPosition;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(node.gameObject, "Rebuild Perk Tree");
            }
#endif
            PerkNodeUI nodeUI = node.GetComponent<PerkNodeUI>() ?? node.gameObject.AddComponent<PerkNodeUI>();
            nodeUI.Initialize(
                perk,
                getUnlockState,
                isPerkPending,
                togglePendingPerk,
                showTooltip,
                hideTooltip,
                refreshRequested);
            spawnedPerkNodes.Add(node.gameObject);
        }
    }
}
