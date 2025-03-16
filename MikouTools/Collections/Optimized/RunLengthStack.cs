using System.Text;

namespace MikouTools.Collections.Optimized
{
    public class RunLengthStack<T>
    {
        // Internal record to store a run-length entry.
        // The Count field is nullable; a null value represents a count of 1 to save memory.
        private record class RunLengthEntry(int? Count, T Value)
        {
            // Making Count mutable so that we can update it when pushing/popping.
            public int? Count { get; set; } = Count;
        }

        // Internal stack holding run-length entries.
        private readonly Stack<RunLengthEntry> values = new();

        /// <summary>
        /// Returns the number of groups (entries) in the stack.
        /// </summary>
        public int GroupCount => values.Count;

        /// <summary>
        ///  Returns the total number of elements in the stack.
        /// For each entry, if Count is null, it is treated as 1.
        /// </summary>
        public int AccurateCount => values.Sum(x => x.Count ?? 1);

        // Utility method to normalize a count (null is interpreted as 1).
        private static int NormalizeCount(int? count) => count ?? 1;

        /// <summary>
        /// Pushes a new element onto the stack.
        /// </summary>
        /// <param name="value"></param>
        public void Push(T value)
        {
            // If the top entry's value equals the new value, increment the run-length.
            if (values.Count > 0 && values.Peek() is RunLengthEntry top && EqualityComparer<T>.Default.Equals(top.Value, value))
            {
                top.Count = NormalizeCount(top.Count) + 1;
            }
            else
            {
                // When pushing a new distinct value, use null for Count (i.e. 1) to save memory.
                values.Push(new RunLengthEntry(null, value));
            }
        }

        /// <summary>
        /// Pops an element from the stack.
        /// f the top entry has a run-length greater than 1, only decrements the count.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T Pop()
        {
            if (values.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            var top = values.Peek();
            if (NormalizeCount(top.Count) > 1)
            {
                top.Count = NormalizeCount(top.Count) - 1;
                return top.Value;
            }
            else
            {
                // Remove the entire entry when the count is 1.
                return values.Pop().Value;
            }
        }

        /// <summary>
        /// Tries to pop an element from the stack.
        /// Returns true if successful, false if the stack is empty.
        /// Note: When the Try** methods fail, they return default(T).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryPop(out T? value)
        {
            if (values.Count == 0)
            {
                value = default;
                return false;
            }

            var top = values.Peek();
            if (NormalizeCount(top.Count) > 1)
            {
                top.Count = NormalizeCount(top.Count) - 1;
                value = top.Value;
            }
            else
            {
                value = values.Pop().Value;
            }
            return true;
        }

        /// <summary>
        /// Peeks at the top element of the stack without removing it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T Peek()
        {
            if (values.Count == 0)
                throw new InvalidOperationException("Stack is empty");

            return values.Peek().Value;
        }

        /// <summary>
        /// Tries to peek at the top element.
        /// Returns true if successful; false if the stack is empty.
        /// Note: When the Try** methods fail, they return default(T).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryPeek(out T? value)
        {
            if (values.Count == 0)
            {
                value = default;
                return false;
            }
            value = values.Peek().Value;
            return true;
        }

        /// <summary>
        /// Clears all entries from the stack.
        /// </summary>
        public void Clear() => values.Clear();

        /// <summary>
        /// Converts the stack to an array.
        /// Each run-length entry is expanded to the repeated value.
        /// </summary>
        /// <returns></returns>
        public T[] ToArray() =>
            values.SelectMany(x => Enumerable.Repeat(x.Value, NormalizeCount(x.Count))).ToArray();

        // Returns a string representation of the stack.
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('[');
            foreach (var item in values)
            {
                sb.Append(item.Value);
                if (NormalizeCount(item.Count) > 1)
                {
                    sb.Append('x');
                    sb.Append(NormalizeCount(item.Count));
                }
                sb.Append(", ");
            }
            if (sb.Length > 1)
            {
                sb.Length -= 2;
            }
            sb.Append(']');
            return sb.ToString();
        }

        // Overrides Equals to compare two RunLengthStack objects.
        public override bool Equals(object? obj)
        {
            if (obj is RunLengthStack<T> other)
            {
                // Compare the number of groups first.
                if (values.Count != other.values.Count)
                    return false;
                // Use SequenceEqual to compare each entry.
                return values.SequenceEqual(other.values);
            }
            return false;
        }

        // Overrides GetHashCode.
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var item in values)
            {
                hash.Add(item.Value);
                hash.Add(NormalizeCount(item.Count));
            }
            return hash.ToHashCode();
        }
    }

}
