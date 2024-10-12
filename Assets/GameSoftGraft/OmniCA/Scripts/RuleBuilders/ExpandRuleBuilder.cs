namespace GameSoftCraft
{
    public class ExpandRuleBuilder : RuleBuilder
    {
        protected override int GetState (bool[] config)
        {
            int active = 0;
            for (int i = 0; i < config.Length; i++) {
                if (i == 4) continue;
                if (config[i]) active++;
            }
            if (active == 1) return 1;
            if (config[4] && active == 1) return 1;

            return 0;
        }
    }
}
