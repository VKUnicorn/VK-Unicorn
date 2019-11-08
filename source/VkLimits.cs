﻿namespace VK_Unicorn
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
    }
}