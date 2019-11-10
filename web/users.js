function loadUsers() {
    clear_workspace();
    start_loading();

    $.getJSON('users', {
    }, function(result) {
        // Добавляем отдельные категории для пользователей
        function addUserCategory(isForNew) {
            $(`
                <div class="row-fluid mx-1" id="users-header-new-${isForNew}">
                    <div class="alert alert-secondary bg-light pt-1 pl-3 pb-0 mt-2 mb-0" role="alert">
                        <h5 class="mb-1">${isForNew ? 'Новые' : 'Все остальные'} пользователи<span class="badge badge-success opaque-5 ml-1" id="badge">0</span></h5>
                    </div>
                </div>
                <div class="row mx-0" id="users-new-${isForNew}">
                </div>
            `).appendTo($('#workspace'));
        };

        let newUsersCount = 0;
        addUserCategory(true);
        let notNewUsersCount = 0;
        addUserCategory(false);

        for (let userExtraInfo of result) {
            let user = userExtraInfo.data;

            let isUserNew = true;

            // Подготавливаем блоки с информацией
            let fullName = user.FirstName + ' ' + user.LastName;
            let age = user.BirthDate == 0 ? 0 : isoTimeToAge(user.BirthDate);
            let isNotInConsentAge = (age > 0) && (age < 16);
            let isUnderage = (age > 0) && (age < 18);
            let ageAsString = age > 0 ? oneFewMany(age, 'год', 'года', 'лет', true) : '';
            let ageElement = ageAsString != '' ? `
                <small>
                    <div class="card-img-overlay small-info">
                        <span class="small-info-box" id="age" data-html="true"><span class="small-info-box-text ${isUnderage ? 'color-error font-weight-bold opaque-9' : ''}">${ageAsString}</span></span>
                    </div>
                </small>
            ` : '';
            let warningElement = isUnderage ? 'bg-error' : '';
            let totalLikes = userExtraInfo.Likes + userExtraInfo.CommentLikes;
            let likesElement = totalLikes > 0 ? '<span id="likes-counter"><i class="lni-heart mr-1"></i><span class="activity-counter">' + totalLikes + '</span></span>' : '';
            let postsElement = userExtraInfo.Posts > 0 ? '<span id="posts-counter"><i class="lni-popup mr-1"></i><span class="activity-counter">' + userExtraInfo.Posts + '</span></span>' : '';
            let commentsElement = userExtraInfo.Comments > 0 ? '<span id="comments-counter"><i class="lni-comment-reply mr-1"></i><span class="activity-counter">' + userExtraInfo.Comments + '</span></span>' : '';

            // Заполняем карточку пользователя
            let userCard = $(`
                <div class="col-sm-2 px-1 py-1">
                    <div class="card ${warningElement}">
                        <div class="card-img-overlay px-1 py-1">
                            <a class="btn btn-success float-left px-1 py-1" id="hide-button"><i class="lni-check-mark-circle size-sm" style="color: white"></i></a>
                            <a class="btn btn-danger float-right px-2 py-2" id="delete-button"><i class="lni-close" style="color: white"></i></a>
                        </div>
                        <a href="${userExtraInfo.URL}" target="_blank">
                            <img class="card-img-top" src="${user.PhotoURL}">
                        </a>
                        ${ageElement}
                        <div class="card-body py-0 px-2">
                            <p class="card-text my-0 text-truncate">${fullName}</p>
                        </div>
                        <div class="card-footer pt-0 px-2" style="padding-bottom: 1px">
                            <div style="padding-top: 2px">
                                <small class="text-muted">
                                    ${likesElement}
                                    ${postsElement}
                                    ${commentsElement}
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            `).appendTo($('#users-new-' + isUserNew));

            // Тултипы
            userCard.find('.delete-user-button').popover({
                trigger: 'hover',
                placement: 'bottom',
                delay: { "show": 450, "hide": 100 },
                content: 'Удалить пользователя навсегда'
            });

            userCard.find('.hide-user-button').popover({
                trigger: 'hover',
                placement: 'bottom',
                delay: { "show": 450, "hide": 100 },
                content: 'Временно скрыть пользователя пока не появится любая новая активность'
            });

            userCard.find('#likes-counter').popover({
                trigger: 'hover',
                placement: 'top',
                delay: { "show": 450, "hide": 100 },
                content: 'Общее количество лайков к записям и комментариям'
            });

            userCard.find('#posts-counter').popover({
                trigger: 'hover',
                placement: 'top',
                delay: { "show": 450, "hide": 100 },
                content: 'Количество записей'
            });

            userCard.find('#comments-counter').popover({
                trigger: 'hover',
                placement: 'top',
                delay: { "show": 450, "hide": 100 },
                content: 'Количество комментариев'
            });

            if (isUnderage) {
                userCard.find('#age').popover({
                    template: '<div class="popover no-weight-limit" role="tooltip"><div class="arrow"></div><h3 class="popover-header"></h3><div class="popover-body"></div></div>',
                    trigger: 'hover',
                    placement: 'top',
                    delay: { "show": 450, "hide": 100 },
                    content:
                        isNotInConsentAge ? 'Пользователь не достиг возраста сексуального согласия<br /><font color=red>УК РФ Статья 134. Половое сношение и иные действия сексуального характера с лицом, не достигшим шестнадцатилетнего возраста<br />УК РФ Статья 135. Развратные действия</font>'
                                          : 'Пользователь не достиг совершеннолетия<br /><font color=red>УК РФ Статья 240.1. Получение сексуальных услуг несовершеннолетнего</font>'
                });
            }

            // Оработчики событий
            userCard.find('#hide-button').click(function(){
                bootbox.confirm({
                    title: "Скрыть?",
                    message: "Вы действительно хотите скрыть пользователя \"" + fullName + "\"?",
                    backdrop: true,
                    callback: function(result) {
                        if (result) {
                            // Отправляем запрос на удаление
                            $.post("hide_user",
                            {
                                id: user.Id
                            },
                            function(data, status) {
                                if (status) {
                                    // Удаляем пользователя из UI
                                    updateBadgeRelative('#users-header-new-' + isUserNew, -1);
                                    userCard.fadeOut();

                                    $.hulla.send("Пользователь \"" + fullName + "\" скрыт", "danger");
                                }
                                else {
                                    $.hulla.send("Не удалось скрыть пользователя \"" + fullName + "\"", "danger");
                                }
                            }).fail(function(result) {
                                $.hulla.send("Не удалось скрыть пользователя \"" + fullName + "\"", "danger");
                            });
                        }
                    }
                });
            });

            userCard.find('#delete-button').click(function(){
                bootbox.confirm({
                    title: "Удалить?",
                    message: "Вы действительно хотите навсегда удалить пользователя \"" + fullName + "\"?",
                    backdrop: true,
                    callback: function(result) {
                        if (result) {
                            // Отправляем запрос на удаление
                            $.post("delete_user",
                            {
                                id: user.Id
                            },
                            function(data, status) {
                                if (status) {
                                    // Удаляем пользователя из UI
                                    updateBadgeRelative('#users-header-new-' + isUserNew, -1);
                                    userCard.fadeOut();

                                    $.hulla.send("Пользователь \"" + fullName + "\" удалён", "danger");
                                }
                                else {
                                    $.hulla.send("Не удалось удалить пользователя \"" + fullName + "\"", "danger");
                                }
                            }).fail(function(result) {
                                $.hulla.send("Не удалось удалить пользователя \"" + fullName + "\"", "danger");
                            });
                        }
                    }
                });
            });

            // Увеличиваем счётчики
            if (isUserNew) {
                ++newUsersCount;
            }
            else {
                ++notNewUsersCount;
            }
        }

        // Заполняем баджи с количеством пользователей
        updateBadgeRelative('#users-header-new-true', newUsersCount);
        updateBadgeRelative('#users-header-new-false', notNewUsersCount);

        finish_loading();

        let recordsCount = result.length;
        if (recordsCount > 0) {
            $.hulla.send(oneFewMany(recordsCount, "Загружен", "Загружено", "Загружено") + " " + oneFewMany(recordsCount, "пользователь", "пользователя", "пользователей", true), "success");
        }
        else {
            $.hulla.send("Не было загружено ни одного пользователя, поэтому было открыто окно настройки сообществ", "success");

            loadGroups();
        }
    }).fail(function(result) {
        finish_loading();

        let responseJSON = result['responseJSON'];
        if (responseJSON === undefined) {
            $.hulla.send("Ошибка при загрузке списка пользователей.<br>Главный модуль программы не запущен или в нём произошла внутренняя ошибка", "danger");
        }
        else {
            $.hulla.send(responseJSON.error, "danger");
        }
    })
}