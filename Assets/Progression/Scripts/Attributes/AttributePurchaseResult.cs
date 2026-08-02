namespace RPGame.Progression
{
    public readonly struct AttributePurchaseResult
    {
        public AttributePurchaseResult(bool success, int spentXP)
        {
            Success = success;
            SpentXP = spentXP;
        }

        public bool Success { get; }
        public int SpentXP { get; }
    }
}
