using System;

namespace GameSoftCraft
{
    /// <summary>
    /// For JSON serialization
    /// </summary>
    [Serializable]
    public struct RuleInfo
    {
        public int[] ruleArray;
        public RuleInfo (int[] ruleArray) => this.ruleArray = ruleArray;
    }
}

