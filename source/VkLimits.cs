namespace VK_Unicorn
{
    class VkLimits
    {
        // Основные лимиты тут https://vk.com/dev/data_limits

        // https://vk.com/dev/groups.getById
        public const int GROUPS_GETBYID_GROUP_IDS = 500;

        // https://vk.com/dev/wall.get
        public const ulong WALL_GET_COUNT = 100;

        // https://vk.com/dev/users.get
        public const int USERS_GET_USER_IDS = 1000;

        // https://vk.com/dev/likes.getList
        public const int LIKES_GET_LIST_COUNT = 100;

        // https://vk.com/dev/execute
        public const int EXECUTE_VK_API_METHODS_COUNT = 25;

        // https://vk.com/dev/wall.getComments
        public const int WALL_GET_COMMENTS_COUNT = 100;
    }
}