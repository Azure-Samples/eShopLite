using Microsoft.AspNetCore.Components.AI;
using Microsoft.Extensions.AI;

namespace Store.Components;

public static class ShoppingConversation
{
    public static UIAgent Create(IChatClient chatClient, AIFunction? cartAction = null) =>
        new(chatClient, options =>
        {
            options.AddGeneratedToolBlocks();
            if (cartAction is not null) { options.RegisterUIAction(cartAction); }
        });
}
