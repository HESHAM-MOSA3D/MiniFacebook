using Microsoft.AspNetCore.SignalR;

namespace MiniFacebook.SignalR
{
    public class SocialHub : Hub
    {
        public async Task SendPost(string user, string content, string imageUrl)
        {
            await Clients.All.SendAsync("ReceivePost", user, content, imageUrl);
        }

        public async Task SendComment(int postId, string user, string content, string imageUrl)
        {
            await Clients.All.SendAsync("ReceiveComment", postId, user, content, imageUrl);
        }

        public async Task UpdatePost(int postId, string content, string imageUrl, string user)
        {
            await Clients.All.SendAsync("PostUpdated", postId, content, imageUrl, user);
        }

        public async Task DeletePost(int postId)
        {
            await Clients.All.SendAsync("PostDeleted", postId);
        }

        public async Task ToggleLike(int? postId, int? commentId, string user, bool isLiked, int likesCount)
        {
            await Clients.All.SendAsync("LikeToggled", postId, commentId, user, isLiked, likesCount);
        }
    }
}