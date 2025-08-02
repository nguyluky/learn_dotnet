

namespace test_api.Models.Responses
{
    public class PaddingResponse<T>
    {
        public int Padding { get; set; } = 0;
        public int Limit { get; set; } = 100;
        public int TotalCount { get; set; } = 0;
        public IEnumerable<T> Items { get; set; } = null!;
    }
}
