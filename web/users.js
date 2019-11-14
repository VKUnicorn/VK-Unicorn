function loadUsers(favorites) {
    clear_workspace();
    start_loading();

    $.getJSON('users', {
        favorites: favorites
    }, function(result) {
        // Позиция тултипа с информацией о пользователе. По умолчанию - справа
        // Если ширина окна браузера слишком маленькая, то будем отображать тултип с информацией о пользователе снизу, а не справа
        let isSmallWindow = $(window).width() < 1200;
        let userShortInfoPopoverOffset = isSmallWindow ? 0 : '50%p - 50% - 2px';
        let userShortInfoPopoverPlecement = isSmallWindow ? 'bottom' : 'right';

        // Тултипы. Здесь объявлены те, которые используются в нескольких местах
        let deletePopover = {
            trigger: 'hover',
            placement: 'bottom',
            html: true,
            delay: { "show": 1100, "hide": 100 },
            content: 'Удалить пользователя навсегда<br><small class="text-muted">Удерживайте кнопку нажатой для удаления</small>'
        };

        let hidePopover = {
            trigger: 'hover',
            placement: 'bottom',
            html: true,
            delay: { "show": 1100, "hide": 100 },
            content: 'Временно скрыть пользователя пока не появится любая новая активность<br><small class="text-muted">Удерживайте кнопку нажатой для скрытия</small>'
        };

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
            let ageWarning = '';
            if (isUnderage) {
                ageWarning = isNotInConsentAge ? getNotInConsentAgeWarning() : getUnderageWarning();
            }
            let cardUnderlayClass = '';
            if (isUserNew) {
                cardUnderlayClass = 'bg-new-user';
            }
            if (userExtraInfo.IsStopWordsFound) {
                cardUnderlayClass = 'bg-stop-words';
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
                <div class="col-auto px-1 py-1">
                    <div class="card ${cardUnderlayClass} user-card">
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

            // Основной тултип с действиями пользователя
            function fillUserActionsCard(paramPostsOrComments, paramLikes, isBigMode) {
                function addAttachmentsFromPost(post) {
                    let result = '';
                    let attachments = JSON.parse(post.Attachments);
                    if (attachments) {
                        let count = 0;
                        for (let attachment of attachments) {
                            result += `
                                <a href="${attachment}" target="_blank">
                                    <img src="${attachment}" class="${isBigMode ? 'attachment-big' : 'attachment'}">
                                </a>
                            `;

                            ++count;

                            // Максимум пять фотографий
                            if (count >= 5) {
                                break;
                            }
                        }
                    }

                    return result;
                }

                function fillNotes() {
                    if (user.Notes) {
                        if (user.Notes != '') {
                            result += `
                                <h6 class="mx-2 mb-1 mt-0">Заметка:</h6>
                                <div class="mx-2">
                                    <span class="block-with-text-2 text-notes"><i class="lni-pencil-alt mr-1 text-dark"></i>${user.Notes}</span>
                                </div>
                            `;
                        }
                    }
                };

                function fillStatus() {
                    if (user.Status) {
                        let status = user.Status.trim();
                        if (status != '') {
                            if (result != '') {
                                result += '<div class="mt-2"></div>';
                            }

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
                                    <span class="block-with-text-1"><i class="lni-link mr-1 text-dark"></i><a href="${site}" target="_blank">${site}</a></span>
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

                function fillPostsAndComments() {
                    if (paramPostsOrComments.length > 0) {
                        if (result != '') {
                            result += '<div class="mt-2"></div>';
                        }

                        result += `
                            <h6 class="mx-2 mb-1 mt-0">Написала:</h6>
                        `;

                        result += `<div class="mx-2">`
                        let index = 0;
                        for (let paramPostOrComment of paramPostsOrComments) {
                            let isPost = paramPostOrComment.Activity.Type == 0;
                            result += `
                                ${index > 0 ? '<hr class="mx-0 my-1">' : ''}
                                <div class="block-with-text-4">
                                    ${isPost ? paramPostOrComment.Post.Content : paramPostOrComment.Comment.Content}
                                </div>
                                ${addAttachmentsFromPost(isPost ? paramPostOrComment.Post : paramPostOrComment.Comment)}
                                <span class="text-muted block-with-text-1 opaque-5">
                                    <i class="${isPost ? "lni-popup" : "lni-comment-reply"} mr-0 text-dark"></i>
                                    <a href="${paramPostOrComment.URL}" target="_blank" class="text-dark">
                                        ${isoTimeToLocalDeltaAsString(paramPostOrComment.Activity.WhenHappened)} назад в сообществе "${paramPostOrComment.Group.Name}"
                                    </a>
                                </span>
                            `;
                            ++index;
                        }

                        result += `</div>`
                    }
                };

                function fillLikes() {
                    if (paramLikes.length > 0) {
                        if (result != '') {
                            result += '<div class="mt-2"></div>';
                        }

                        result += `
                            <h6 class="mx-2 mb-1 mt-0">Поставила лайк:</h6>
                        `;

                        result += `<div class="mx-2">`
                        let index = 0;
                        for (let paramLike of paramLikes) {
                            let isLikeToPost = paramLike.Activity.Type == 1;
                            result += `
                                ${index > 0 ? '<hr class="mx-0 my-1">' : ''}
                                <div class="block-with-text-4 text-like">
                                    ${isLikeToPost ? paramLike.Post.Content : paramLike.Comment.Content}
                                </div>
                                ${addAttachmentsFromPost(isLikeToPost ? paramLike.Post : paramLike.Comment)}
                                <span class="text-muted block-with-text-1 opaque-5">
                                    <i class="lni-heart mr-0"></i>
                                    <a href="${paramLike.URL}" target="_blank" class="text-dark">
                                        ${isoTimeToLocalDeltaAsString(paramLike.Activity.WhenHappened)} назад в сообществе "${paramLike.Group.Name}"
                                    </a>
                                </span>
                            `;
                            ++index;
                        }

                        result += `</div>`
                    }
                };

                var result = '';
                fillNotes();
                fillStatus();
                fillCity();
                fillSite();
                fillPhone();
                fillPostsAndComments();
                fillLikes();
                return result;
            }

            // Превью нескольких последних действий пользователя
            userCard.find('#user').popover({
                template: getPopoverTemplateWithClass("user-short-info", "px-0"),
                trigger: 'hover',
                placement: userShortInfoPopoverPlecement,
                animation: false,
                html: true,
                offset: userShortInfoPopoverOffset,
                delay: { "show": 50, "hide": 25 },
                content: function() {
                    return fillUserActionsCard(recentPosts, recentLikes, false);
                }
            });

            // Тултипы в зависимости от контекста
            if (!user.IsDeactivated) {
                userCard.find('#hide-button').popover(hidePopover);
            }

            if (isUnderage) {
                userCard.find('#age').popover({
                    template: getPopoverTemplateWithClass("no-weight-limit"),
                    trigger: 'hover',
                    placement: 'top',
                    html: true,
                    delay: { "show": 450, "hide": 100 },
                    content: isNotInConsentAge ? getNotInConsentAgeWarning() : getUnderageWarning()
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

                            // Закрываем модальное окно, если было открыто
                            $('#user-full-info-modal').modal('hide');

                            $.hulla.send("Пользователь \"" + fullName + "\" скрыт", "success");
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

                        // Закрываем модальное окно, если было открыто
                        $('#user-full-info-modal').modal('hide');

                        $.hulla.send("Пользователь \"" + fullName + "\" удалён", "success");
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

            // Событие клика на карточку пользователя. Если кликаем левой кнопкой мыши - открываем полную инорфмацию.
            // Если кликаем средней кнопкой, то открываем сразу профиль пользователя в новой вкладке
            userCard.find('#user').click(function(e) {
                e.preventDefault();

                $.getJSON('user_activities', {
                    id: user.Id,
                    // Если зажат shift, то загружаем вообще всю активность, без лимита по времени
                    noTimeLimit: event.shiftKey
                }, function(result) {
                    let allPosts = result[0].Posts;
                    let allLikes = result[0].Likes;

                    // Подготавливаем модальное окно с подробной информацией о пользователе
                    let userFullInfoModal = $('#user-full-info-modal');
                    let userFullInfoModalContent = userFullInfoModal.find("#user-full-info");

                    function RebuildFullInfoModalContent() {
                        userFullInfoModalContent.empty();

                        // Заполняем информацию с фотографией и общими данными
                        userFullInfoModalContent.append(`
                            <div class="row">
                                <div class="col-3 pr-0">
                                    <a href="${userExtraInfo.URL}" target="_blank">
                                        <img src="${user.PhotoURL}" class="user-full-info-photo">
                                    </a>
                                </div>
                                <div class="col-9 pl-1">
                                    <h5 class="mx-2 mb-1 mt-0 block-with-text-1">${fullName}${ageAsString != '' ? (', ' + ageAsString) : ''}</h5>
                                    <hr class="ml-2 my-2">
                                    <div class="mx-2">${ageWarning}</div>
                                    ${ageWarning != '' ? '<hr class="ml-2 my-2">' : ''}
                                    ${fillUserActionsCard(allPosts, allLikes, true)}
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-3 pr-0">
                                </div>
                                <div class="col-9 pl-1">
                                    <hr class="ml-2 my-2">
                                    <div class="mr-0 float-right">
                                        ${user.IsDeactivated ? '' : '<button type="button" class="btn btn-success opaque-8" id="hide-button">Скрыть</button>'}
                                        <button type="button" class="btn btn-danger opaque-8" id="delete-button">Удалить</button>
                                        <button type="button" class="btn btn-notes opaque-6" id="notes-button">Заметка</button>
                                        <button type="button" class="btn btn-light" data-dismiss="modal">Закрыть</button>
                                    </div>
                                </div>
                            </div>
                        `);

                        // Тултип и действие на кнопку "Скрыть"
                        if (!user.IsDeactivated) {
                            let hideButton = userFullInfoModalContent.find('#hide-button');
                            hideButton.popover(hidePopover);
                            hideButton.mayTriggerLongClicks().on('longClick', function(data) {
                                userCard.find('#hide-button').trigger('longClick');
                            });
                        }

                        // Тултип и действие на кнопку "Заметка"
                        let notesButton = userFullInfoModalContent.find('#notes-button');
                        notesButton.popover({
                            template: getPopoverTemplateWithClass("no-weight-limit"),
                            trigger: 'hover',
                            placement: 'bottom',
                            html: true,
                            delay: { "show": 500, "hide": 100 },
                            content: 'Редактировать заметку о пользователе'
                        });
                        notesButton.click(function() {
                            bootbox.prompt({
                                title: "Добавить заметку",
                                message: "Введите заметку о пользователе:",
                                inputType: 'textarea',
                                placeholder: "Заметка о пользователе",
                                value: user.Notes,
                                backdrop: true,
                                callback: function(result) {
                                    if (result != null) {
                                        // Отправляем запрос на изменение заметки
                                        $.post("user_notes",
                                        {
                                            id: user.Id,
                                            notes: result,
                                        },
                                        function(data, status) {
                                            if (status) {
                                                user.Notes = result;
                                                RebuildFullInfoModalContent();
                                            }
                                            else {
                                                $.hulla.send("Не удалось изменить заметку у пользователя \"" + fullName + "\"", "danger");
                                            }
                                        }).fail(function(result) {
                                            $.hulla.send("Не удалось изменить заметку у пользователя \"" + fullName + "\"", "danger");
                                        });
                                    }
                                }
                            });
                        });

                        // Тултип и действие на кнопку "Удалить"
                        let deleteButton = userFullInfoModalContent.find('#delete-button');
                        deleteButton.popover(deletePopover);
                        deleteButton.mayTriggerLongClicks().on('longClick', function(data) {
                            userCard.find('#delete-button').trigger('longClick');
                        });
                    }

                    // Обновляем содержимое окна
                    RebuildFullInfoModalContent();

                    // Открываем модальное окно
                    userFullInfoModal.modal();
                }).fail(function(result) {
                    $.hulla.send("Ошибка при загрузке полной информации о пользователе. Главный модуль программы не запущен или пользователь был удалён", "danger");
                })
            });

            // Увеличиваем счётчики
            if (isUserNew) {
                ++newUsersCount;
            }
            else {
                ++notNewUsersCount;
            }
        }

        // Общие тултипы на все элементы
        $('*[id=delete-button]').popover(deletePopover);

        $('*[id=likes-counter]').popover({
            trigger: 'hover',
            placement: 'top',
            delay: { "show": 450, "hide": 100 },
            content: 'Общее количество лайков к записям и комментариям'
        });

        $('*[id=posts-counter]').popover({
            trigger: 'hover',
            placement: 'top',
            delay: { "show": 450, "hide": 100 },
            content: 'Количество записей'
        });

        $('*[id=comments-counter]').popover({
            trigger: 'hover',
            placement: 'top',
            delay: { "show": 450, "hide": 100 },
            content: 'Количество комментариев'
        });

        $('*[id=favorite]').popover({
            trigger: 'hover',
            placement: 'top',
            delay: { "show": 1000, "hide": 100 },
            content: 'Добавить или удалить из избранного'
        });

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

        // Добавляем пока скрытое модальное окно с подробной информацией о пользователе
        $(`
            <div class="modal fade" id="user-full-info-modal" tabindex="-1">
                <div class="modal-dialog modal-xl">
                    <div class="modal-content ml-1">
                        <div class="modal-body" id="user-full-info">
                        </div>
                    </div>
                </div>
            </div>
        `).appendTo($('#workspace'));
    }).fail(function(result) {
        finish_loading();

        $.hulla.send("Ошибка при загрузке списка пользователей.<br>Главный модуль программы не запущен или в нём произошла внутренняя ошибка", "danger");
    })
}