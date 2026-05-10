using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGame.UI.Jobs
{
    public sealed class PerkTreeConnectionsGraphic : MaskableGraphic
    {
        [SerializeField] private float lineThickness = 4f;
        [SerializeField] private Color lockedColor = new(0.25f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color availableColor = new(0.95f, 0.82f, 0.35f, 1f);
        [SerializeField] private Color pendingColor = new(0.35f, 0.65f, 1f, 1f);
        [SerializeField] private Color unlockedColor = new(0.35f, 0.85f, 0.48f, 1f);

        private readonly List<PerkTreeConnection> connections = new();

        public IReadOnlyList<PerkTreeConnection> Connections => connections;

        public override Material defaultMaterial => defaultGraphicMaterial;

        protected override void Awake()
        {
            base.Awake();
            ClearStencilIncompatibleMaterial();
        }

        public void SetConnections(IReadOnlyList<PerkTreeConnection> newConnections)
        {
            connections.Clear();

            if (newConnections != null)
            {
                for (int i = 0; i < newConnections.Count; i++)
                {
                    connections.Add(newConnections[i]);
                }
            }

            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            float halfThickness = Mathf.Max(1f, lineThickness) * 0.5f;
            for (int i = 0; i < connections.Count; i++)
            {
                AddLine(vertexHelper, connections[i].From, connections[i].To, halfThickness, GetColor(connections[i].State));
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            lineThickness = Mathf.Max(1f, lineThickness);
            ClearStencilIncompatibleMaterial();
            SetVerticesDirty();
        }

        private void ClearStencilIncompatibleMaterial()
        {
            if (material != null && !material.HasProperty("_Stencil"))
            {
                material = null;
            }
        }

        private Color GetColor(PerkTreeConnectionState state)
        {
            return state switch
            {
                PerkTreeConnectionState.Available => availableColor,
                PerkTreeConnectionState.Pending => pendingColor,
                PerkTreeConnectionState.Unlocked => unlockedColor,
                _ => lockedColor
            };
        }

        private void AddLine(VertexHelper vertexHelper, Vector2 from, Vector2 to, float halfThickness, Color lineColor)
        {
            Vector2 direction = to - from;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Vector2 normal = new Vector2(-direction.y, direction.x).normalized * halfThickness;
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(from - normal, lineColor, Vector2.zero);
            vertexHelper.AddVert(from + normal, lineColor, Vector2.zero);
            vertexHelper.AddVert(to + normal, lineColor, Vector2.zero);
            vertexHelper.AddVert(to - normal, lineColor, Vector2.zero);
            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }
    }
}
