namespace test_api.Models.Dtos
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int ProductId { get; set; }
        public int OrderItemId { get; set; }
    }

    public class CreateCommentDto
    {
        public string Content { get; set; } = null!;
        public int Rating { get; set; } // 1-5 stars
        public int ProductId { get; set; }
        public int OrderItemId { get; set; }
    }

    public class UpdateCommentDto
    {
        public string? Content { get; set; }
        public int? Rating { get; set; }
    }
}
