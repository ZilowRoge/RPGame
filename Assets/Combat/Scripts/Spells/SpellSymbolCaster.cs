using System;
using System.Collections;
using System.Collections.Generic;
using RPGame.Core.Spells;
using RPGame.Core.Spells.Symbols;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class SpellSymbolCaster : SymbolReceiverBase
    {
        public event Action<Spell> SpellSelected;

        [SerializeField] private SpellSymbolEntry[] spellsBySymbol;
        [SerializeField] private int[] terminatorSymbolIds;
        [SerializeField, Min(0f)] private float sequenceTimeout;

        private readonly List<int> currentSequence = new();
        private readonly HashSet<int> terminatorSymbols = new();
        private SpellSequenceResolver spellSequenceResolver;
        private Coroutine sequenceTimeoutCoroutine;

        private void Awake()
        {
            spellSequenceResolver = new SpellSequenceResolver(spellsBySymbol);
            if (terminatorSymbolIds == null)
            {
                return;
            }

            for (int i = 0; i < terminatorSymbolIds.Length; i++)
            {
                terminatorSymbols.Add(terminatorSymbolIds[i]);
            }

            Debug.Log($"SpellSymbolCaster initialized. Terminators: [{string.Join(", ", terminatorSymbols)}], timeout: {sequenceTimeout}.", this);
        }

        public override void OnDrawingStarted()
        {
            Debug.Log($"Spell drawing started. Current sequence before timeout cancel: [{string.Join(", ", currentSequence)}].", this);
            CancelSequenceTimeout();
        }

        public override void ReceiveSymbol(SymbolRecognitionResult result)
        {
            if (!result.IsRecognized)
            {
                ClearSequence();
                Debug.LogWarning("Symbol spell selection skipped because symbol was not recognized.", this);
                return;
            }

            currentSequence.Add(result.SymbolId);
            Debug.Log($"Spell symbol received: {result.SymbolId}. Sequence: [{string.Join(", ", currentSequence)}]. Terminator: {terminatorSymbols.Contains(result.SymbolId)}.", this);
            if (!terminatorSymbols.Contains(result.SymbolId))
            {
                StartSequenceTimeout();
                return;
            }

            if (spellSequenceResolver.TryResolve(currentSequence, out Spell spell))
            {
                Debug.Log($"Spell sequence resolved: [{string.Join(", ", currentSequence)}] -> {spell.name}.", this);
                SpellSelected?.Invoke(spell);
            }
            else
            {
                Debug.LogWarning($"Spell sequence did not resolve: [{string.Join(", ", currentSequence)}].", this);
            }

            ClearSequence();
        }

        private void OnDisable()
        {
            ClearSequence();
        }

        private void StartSequenceTimeout()
        {
            CancelSequenceTimeout();
            Debug.Log($"Spell sequence timeout started for {sequenceTimeout} seconds: [{string.Join(", ", currentSequence)}].", this);
            sequenceTimeoutCoroutine = StartCoroutine(ClearSequenceAfterTimeout());
        }

        private IEnumerator ClearSequenceAfterTimeout()
        {
            yield return new WaitForSeconds(sequenceTimeout);
            sequenceTimeoutCoroutine = null;
            Debug.LogWarning($"Spell sequence timed out and was cleared: [{string.Join(", ", currentSequence)}].", this);
            currentSequence.Clear();
        }

        private void ClearSequence()
        {
            currentSequence.Clear();
            CancelSequenceTimeout();
        }

        private void CancelSequenceTimeout()
        {
            if (sequenceTimeoutCoroutine == null)
            {
                return;
            }

            StopCoroutine(sequenceTimeoutCoroutine);
            sequenceTimeoutCoroutine = null;
        }
    }
}
