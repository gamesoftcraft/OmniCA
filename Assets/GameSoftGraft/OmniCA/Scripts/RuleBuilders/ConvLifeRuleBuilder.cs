namespace GameSoftCraft
{
    public class ConvLifeRuleBuilder : RuleBuilder
    {
        protected override int GetState (bool[] config)
        {
            int active = 0;
            for (int i = 0; i < config.Length; i++) {
                if (i == 4) continue;
                if (config[i]) active++;
            }

            if (active == 3) return 1;
            if (active > 3) return 0;
            if (active < 2) return 0;
            if (config[4] && active == 2) return 1;

            return 0;
        }
    }
}
