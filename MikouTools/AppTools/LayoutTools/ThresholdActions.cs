namespace MikouTools.AppTools.LayoutTools
{
    public class ThresholdActionManager
    {
        private class ThresholdAction
        {
            public double Threshold { get; }
            public Action OnCrossedAbove { get; }
            public Action OnCrossedBelow { get; }

            public bool IsAbove { get; private set; } = false;

            public ThresholdAction(double threshold, Action onCrossedAbove, Action onCrossedBelow)
            {
                Threshold = threshold;
                OnCrossedAbove = onCrossedAbove;
                OnCrossedBelow = onCrossedBelow;
            }

            public bool Evaluate(double value)
            {
                bool shouldBeAbove = value >= Threshold;
                if (shouldBeAbove != IsAbove)
                {
                    IsAbove = shouldBeAbove;
                    if (shouldBeAbove)
                        OnCrossedAbove?.Invoke();
                    else
                        OnCrossedBelow?.Invoke();
                }
                return shouldBeAbove;
            }
        }

        private readonly List<ThresholdAction> thresholdActions = new List<ThresholdAction>();

        public bool IsSort { get; private set; } = false;
        public void Sort()
        {
            if (!IsSort)
            {
                thresholdActions.Sort((a, b) => a.Threshold.CompareTo(b.Threshold));
                IsSort = true;
            }
        }

        public void EvaluateAll(double value)
        {
            Sort();

            foreach (var action in thresholdActions)
            {
                action.Evaluate(value);
            }
        }

        public void AddThreshold(double threshold, Action onCrossedAbove, Action onCrossedBelow, bool autoSort = true)
        {
            thresholdActions.Add(new ThresholdAction(threshold, onCrossedAbove, onCrossedBelow));
            if (autoSort)
            {
                Sort();
            }
            else
            {
                IsSort = false;
            }
        }
    }
}
