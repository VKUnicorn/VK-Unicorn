function loadUsers(favorites) {
    clear_workspace();
    start_loading();

    $.getJSON('users', {
        favorites: favorites
    }, function(result) {
        // Позиция тултипа с информацией о пользователе. По умолчанию - справа
        // Если ширина окна браузера слишком маленькая, то будем отображать тултип с информацией о пользователе снизу, а не справа
        let smallWindow = $(window).width() < 1200;
        let userShortInfoPopoverOffset = smallWindow ? 0 : '50%p - 50% - 2px';
        let userShortInfoPopoverPlecement = smallWindow ? 'bottom' : 'right';

        // Добавляем отдельные категории для пользователей
        function addUserCategory(isForNew) {
            $(`
                <div id="users-header-new-${isForNew}">
                    <h5 class="ml-1 mb-1 mt-2">${isForNew ? 'Новые' : 'Все остальные'}${favorites ? ' избранные' : ''} пользователи<span class="badge badge-success opaque-5 ml-1" id="badge">0</span></h5>
                    <hr class="mx-1 my-0">
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
            let recentPosts = userExtraInfo.RecentPosts;
            let recentLikes = userExtraInfo.RecentLikes;
            let whenAddedToLocalDaysDelta = isoTimeToLocalDaysDelta(user.WhenAdded);
            let isUserNew = (whenAddedToLocalDaysDelta >= 0) && (whenAddedToLocalDaysDelta < 5);

            // Подготавливаем блоки с информацией
            let fullName = user.FirstName + ' ' + user.LastName;
            let age = user.BirthDate == 0 ? 0 : isoTimeToAge(user.BirthDate);
            let isNotInConsentAge = (age > 0) && (age < 16);
            let isUnderage = (age > 0) && (age < 18);
            let ageAsString = age > 0 ? oneFewMany(age, 'год', 'года', 'лет', true) : '';
            let ageElement = (ageAsString != '') ? `
                <small>
                    <div class="card-img-overlay small-info">
                        <span class="small-info-box" id="age"><span class="small-info-box-text ${isUnderage ? 'color-error font-weight-bold opaque-9' : ''}">${ageAsString}</span></span>
                    </div>
                </small>
            ` : '';
            let cardUnderlayClass = '';
            if (isUserNew) {
                cardUnderlayClass = 'bg-new-user';
            }
            if (isUnderage) {
                cardUnderlayClass = 'bg-error';
            }
            let totalLikes = userExtraInfo.Likes + userExtraInfo.CommentLikes;
            let likesElement = totalLikes > 0 ? '<span id="likes-counter"><i class="lni-heart mr-1"></i><span class="activity-counter">' + totalLikes + '</span></span>' : '';
            let postsElement = userExtraInfo.Posts > 0 ? '<span id="posts-counter"><i class="lni-popup mr-1"></i><span class="activity-counter">' + userExtraInfo.Posts + '</span></span>' : '';
            let commentsElement = userExtraInfo.Comments > 0 ? '<span id="comments-counter"><i class="lni-comment-reply mr-1"></i><span class="activity-counter">' + userExtraInfo.Comments + '</span></span>' : '';

            // Заполняем карточку пользователя
            let userCard = $(`
                <div class="col-sm-2 px-1 py-1">
                    <div class="card ${cardUnderlayClass}">
                        <div class="card-img-overlay px-1 py-1">
                            ${!user.IsDeactivated ? '<a class="btn btn-success float-left px-1 py-1" id="hide-button"><i class="lni-check-mark-circle size-sm text-white"></i></a>' : ''}
                            <a class="btn btn-danger float-right px-2 py-2" id="delete-button"><i class="lni-close text-white"></i></a>
                        </div>
                        <a href="${userExtraInfo.URL}" target="_blank" id="user">
                            <img class="card-img-top" src="${user.PhotoURL}">
                        </a>
                        ${ageElement}
                        <div class="card-body py-0 px-2">
                            <p class="card-text my-0 text-truncate">${fullName}</p>
                        </div>
                        <div class="card-footer pt-0 pl-2 pb-p-1 pr-p-3">
                            <div class="float-left pt-p-2">
                                <small class="text-muted">
                                    ${likesElement}
                                    ${postsElement}
                                    ${commentsElement}
                                </small>
                            </div>
                            <div class="float-right pt-p-3">
                                <i class="${user.IsFavorite ? 'lni-star-filled text-warning stroke-red' : 'lni-star text-muted'} opaque-4" id="favorite"></i>
                            </div>
                        </div>
                    </div>
                </div>
            `).appendTo($('#users-new-' + isUserNew));

            // Основной тултип с недавними действиями пользователя
            function fillUserRecentActionsCard() {
                function fillStatus() {
                    if (user.Status) {
                        let status = user.Status.trim();
                        if (status != '') {
                            result += `
                                <h6 class="mx-2 mb-1 mt-0">Статус:</h6>
                                <div class="mx-2">
                                    <span class="block-with-text-2"><i class="lni-chevron-right mr-1 text-dark"></i>${status}</span>
                                </div>
                            `;
                        }
                    }
                };

                function fillCity() {
                    if (userExtraInfo.IsDifferentCity) {
                        if (user.CityName) {
                            let cityName = user.CityName.trim();
                            if (cityName != '') {
                                if (result != '') {
                                    result += '<div class="mt-2"></div>';
                                }

                                result += `
                                    <h6 class="mx-2 mb-1 mt-0">Город:</h6>
                                    <div class="mx-2">
                                        <span class="block-with-text-1"><i class="lni-map-marker mr-1 text-dark"></i>${cityName}</span>
                                    </div>
                                `;
                            }
                        }
                    }
                }

                function fillSite() {
                    if (user.Site) {
                        let site = user.Site.trim();
                        if (site != '') {
                            if (result != '') {
                                result += '<div class="mt-2"></div>';
                            }

                            result += `
                                <h6 class="mx-2 mb-1 mt-0">Сайт:</h6>
                                <div class="mx-2">
                                    <span class="block-with-text-1"><i class="lni-link mr-1 text-dark"></i><a href="${site}">${site}</a></span>
                                </div>
                            `;
                        }
                    }
                };

                function fillPhone() {
                    if (user.MobilePhone || user.HomePhone) {
                        let mobilePhone = user.MobilePhone.trim();
                        let homePhone = user.HomePhone.trim();
                        if ((mobilePhone != '') || (homePhone != '')) {
                            if (result != '') {
                                result += '<div class="mt-2"></div>';
                            }

                            if (mobilePhone != '') {
                                mobilePhone = '<span class="block-with-text-1"><i class="lni-phone-handset mr-1 text-dark"></i>' + mobilePhone + '</span>';
                            }

                            if (homePhone != '') {
                                homePhone = '<span class="block-with-text-1"><i class="lni-phone mr-1 text-dark"></i>' + homePhone + '</span>';
                            }

                            result += `
                                <h6 class="mx-2 mb-1 mt-0">Телефон:</h6>
                                <div class="mx-2">
                                    ${mobilePhone}
                                    ${homePhone}
                                </div>
                            `;
                        }
                    }
                };

                function fillRecentPosts() {
                    if (recentPosts.length > 0) {
                        if (result != '') {
                            result += '<div class="mt-2"></div>';
                        }

                        result += `
                            <h6 class="mx-2 mb-1 mt-0">Написала:</h6>
                        `;

                        result += `<div class="mx-2">`
                        let index = 0;
                        for (let recentPost of recentPosts) {
                            result += `
                                ${index > 0 ? '<hr class="mx-0 my-1">' : ''}
                                <div class="block-with-text-4">
                                    ${recentPost.Post.Content}
                                </div>
                                <span class="text-muted block-with-text-1 opaque-5"><i class="lni-popup mr-1 text-dark"></i>${isoTimeToLocalDeltaAsString(recentPost.Activity.WhenHappened)} назад в сообществе "${recentPost.Group.Name}"</span>
                            `;
                            ++index;
                        }

                        result += `</div>`
                    }
                };

                function fillRecentLikes() {
                    if (recentLikes.length > 0) {
                        if (result != '') {
                            result += '<div class="mt-2"></div>';
                        }

                        result += `
                            <h6 class="mx-2 mb-1 mt-0">Поставила лайк:</h6>
                        `;

                        result += `<div class="mx-2">`
                        let index = 0;
                        for (let recentLike of recentLikes) {
                            result += `
                                ${index > 0 ? '<hr class="mx-0 my-1">' : ''}
                                <div class="block-with-text-4 text-like">
                                    ${recentLike.Post.Content}
                                </div>
                                <span class="text-muted block-with-text-1 opaque-5"><i class="lni-heart mr-1 text-like"></i>${isoTimeToLocalDeltaAsString(recentLike.Activity.WhenHappened)} назад в сообществе "${recentLike.Group.Name}"</span>
                            `;
                            ++index;
                        }

                        result += `</div>`
                    }
                };

                var result = '';
                fillStatus();
                fillCity();
                fillSite();
                fillPhone();
                fillRecentPosts();
                fillRecentLikes();
                return result;
            }

            userCard.find('#user').popover({
                template: getPopoverTemplateWithClass("user-short-info", "px-0"),
                trigger: 'hover',
                placement: userShortInfoPopoverPlecement,
                animation: false,
                html: true,
                offset: userShortInfoPopoverOffset,
                delay: { "show": 50, "hide": 25 },
                content: fillUserRecentActionsCard()
            });

            // Тултипы
            userCard.find('#delete-button').popover({
                trigger: 'hover',
                placement: 'bottom',
                html: true,
                delay: { "show": 1100, "hide": 100 },
                content: 'Удалить пользователя навсегда<br /><small class="text-muted">Удерживайте кнопку нажатой для удаления</small>'
            });

            if (!user.IsDeactivated) {
                userCard.find('#hide-button').popover({
                    trigger: 'hover',
                    placement: 'bottom',
                    html: true,
                    delay: { "show": 1100, "hide": 100 },
                    content: 'Временно скрыть пользователя пока не появится любая новая активность<br /><small class="text-muted">Удерживайте кнопку нажатой для удаления</small>'
                });
            }

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

            userCard.find('#favorite').popover({
                trigger: 'hover',
                placement: 'top',
                delay: { "show": 1000, "hide": 100 },
                content: 'Добавить или удалить из избранного'
            });

            if (isUnderage) {
                userCard.find('#age').popover({
                    template: getPopoverTemplateWithClass("no-weight-limit"),
                    trigger: 'hover',
                    placement: 'top',
                    html: true,
                    delay: { "show": 450, "hide": 100 },
                    content:
                        isNotInConsentAge ? 'Пользователь не достиг возраста сексуального согласия<br /><font color=red>УК РФ Статья 134. Половое сношение и иные действия сексуального характера с лицом, не достигшим шестнадцатилетнего возраста<br />УК РФ Статья 135. Развратные действия</font>'
                                          : 'Пользователь не достиг совершеннолетия<br /><font color=red>УК РФ Статья 240.1. Получение сексуальных услуг несовершеннолетнего</font>'
                });
            }

            // Оработчики событий
            if (!user.IsDeactivated) {
                userCard.find('#hide-button').mayTriggerLongClicks().on('longClick', function(data) {
                    // Отправляем запрос на скрытие
                    $.post("hide_user",
                    {
                        id: user.Id
                    },
                    function(data, status) {
                        if (status) {
                            // Удаляем пользователя из UI
                            userCard.fadeOut();

                            // Обновляем счётчик в бадже
                            updateBadgeRelative('#users-header-new-' + isUserNew, -1);

                            $.hulla.send("Пользователь \"" + fullName + "\" скрыт", "danger");
                        }
                        else {
                            $.hulla.send("Не удалось скрыть пользователя \"" + fullName + "\"", "danger");
                        }
                    }).fail(function(result) {
                        $.hulla.send("Не удалось скрыть пользователя \"" + fullName + "\"", "danger");
                    });
                });
            }

            userCard.find('#delete-button').mayTriggerLongClicks().on('longClick', function(data) {
                // Отправляем запрос на удаление
                $.post("delete_user",
                {
                    id: user.Id
                },
                function(data, status) {
                    if (status) {
                        // Удаляем пользователя из UI
                        userCard.fadeOut();

                        // Обновляем счётчик в бадже
                        updateBadgeRelative('#users-header-new-' + isUserNew, -1);

                        $.hulla.send("Пользователь \"" + fullName + "\" удалён", "danger");
                    }
                    else {
                        $.hulla.send("Не удалось удалить пользователя \"" + fullName + "\"", "danger");
                    }
                }).fail(function(result) {
                    $.hulla.send("Не удалось удалить пользователя \"" + fullName + "\"", "danger");
                });
            });

            let favoriteElement = userCard.find('#favorite');
            favoriteElement.click(function() {
                // Отправляем запрос на изменение статуса "в избранном"
                $.post("favorite_user",
                {
                    id: user.Id,
                    favorite: !user.IsFavorite,
                },
                function(data, status) {
                    if (status) {
                        // Изменяем статус избранности
                        user.IsFavorite = !user.IsFavorite;

                        // Обновляем интерфейс
                        favoriteElement.removeClass();
                        favoriteElement.addClass(user.IsFavorite ? "lni-star-filled text-warning stroke-red" : "lni-star text-muted");
                        favoriteElement.addClass("opaque-4");
                    }
                    else {
                        $.hulla.send("Не удалось изменить статус избранного у пользователя \"" + fullName + "\"", "danger");
                    }
                }).fail(function(result) {
                    $.hulla.send("Не удалось изменить статус избранного у пользователя \"" + fullName + "\"", "danger");
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
            $.hulla.send(oneFewMany(recordsCount, "Загружен", "Загружено", "Загружено") + " " + recordsCount + (favorites ? ' ' + oneFewMany(recordsCount, "избранный", "избранных", "избранных") : '') + " " + oneFewMany(recordsCount, "пользователь", "пользователя", "пользователей"), "success");
        }
        else {
            if (!favorites) {
                $.hulla.send("Не было загружено ни одного пользователя, поэтому было открыто окно настройки сообществ", "success");

                loadGroups();
            } else {
                $.hulla.send("Не было загружено ни одного пользователя. Пользователей можно добавлять в избранное по клику на иконку звёздочки", "success");
            }
        }
    }).fail(function(result) {
        finish_loading();

        $.hulla.send("Ошибка при загрузке списка пользователей.<br>Главный модуль программы не запущен или в нём произошла внутренняя ошибка", "danger");
    })
}