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
                        <h5 class="mb-1">${isForNew ? 'Новые ' : 'Все остальные '} пользователи<span class="badge badge-success opaque-5 ml-1" id="badge-users-new-${isForNew}"></span></h5>
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
            let age = user.BirthDate == 0 ? '' : isoTimeToAgeAsString(user.BirthDate);

            let ageOverlay = age != '' ? `
                <div class="card-img-overlay small-info">
                    <span class="small-info-box">${age}</span>
                </div>
            ` : '';

            // Заполняем карточку пользователя
            let userCard = $(`
                <div class="col-sm-2 px-1 py-1">
                    <div class="card">
                        <div class="card-img-overlay px-1 py-1">
                            <a class="btn btn-success float-left px-1 py-1 hide-user-button"><i class="lni-check-mark-circle size-sm" style="color: white"></i></a>
                            <a class="btn btn-danger float-right px-2 py-2 delete-user-button"><i class="lni-close" style="color: white"></i></a>
                        </div>
                        <a href="${userExtraInfo.URL}" target="_blank">
                            <img class="card-img-top" src="${user.PhotoURL}">
                        </a>
                        ${ageOverlay}
                        <div class="card-body py-0 px-2">
                            <p class="card-text my-0 text-truncate">${user.FirstName} ${user.LastName}</p>
                        </div>
                        <div class="card-footer pt-0 px-2" style="padding-bottom: 1px">
                            <div style="padding-top: 2px">
                                <small class="text-muted">
                                    <i class="lni-heart mr-1"></i><span class="activity-counter">100</span>
                                    <i class="lni-popup mr-1"></i><span class="activity-counter">200</span>
                                    <i class="lni-comment-reply mr-1"></i><span class="activity-counter">300</span>
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            `).appendTo($('#users-new-' + isUserNew));
            /*
            let userCard = $(`
                <div class="col-sm-3 px-1 py-1">
                    <div class="card ${warningElement}">
                        <div class="card-img-overlay px-1 py-1">
                            <a class="btn btn-danger float-right px-2 py-2 delete-group-button"><i class="lni-close" style="color: white"></i></a>
                        </div>
                        <a href="${groupExtraInfo.URL}" target="_blank">
                            <img class="card-img-top" src="${group.PhotoURL}">
                        </a>
                        <div class="card-img-overlay small-info">
                            <span class="small-info-box group-results">${groupExtraInfo.Efficiency}</span>
                        </div>
                        <div class="card-body py-0 px-2">
                            <p class="card-text my-0 text-truncate" id="group-name">${group.Name}</p>
                        </div>
                        <div class="card-footer pt-0 px-2" style="padding-bottom: 1px">
                            <div class="float-left" id="last-activity" data-html="true"><small class="text-muted">${lockElement}<i class="lni-pulse mr-1"></i>${isoTimeToLocalDeltaAsString(group.LastActivity)}</small></div>
                            <div class="float-right" id="last-scanned"><small class="text-muted"><i class="lni-reload" style="padding-right: 2px"></i>${isoTimeToLocalDeltaAsString(group.LastScanned)}</small></div>
                        </div>
                    </div>
                </div>

                <div class="col-sm-2 px-1 py-1" id="user-holder" data-user-id="$USER_ID$">
                    <div class="card $USER_UNDERLAY$">
                        <div class="card-img-overlay px-1 py-1">
                            <a href=# class="btn btn-success float-left px-1 py-1"><i class="lni-check-mark-circle size-sm"></i></a>
                            <a href=# class="btn btn-danger float-right px-2 py-2"><i class="lni-close"></i></a>
                        </div>
                        <img class="card-img-top" src="USER_PHOTO_URL$">
                        <div class="card-img-overlay small-info">
                            <span class="small-info-box">$USER_AGE$</span>
                        </div>
                        <div class="card-body py-0 px-2">
                            <p class="card-text my-0 text-truncate">$USER_FIRST_NAME$ $USER_LAST_NAME$</p>
                            <i class="lni-heart size-sm"></i>
                            <i class="lni-popup size-sm"></i>
                            <i class="lni-comment-reply size-sm"></i>
                        </div>
                    </div>
                </div>
            `).appendTo($('#workspace'));
            */

            // Увеличиваем счётчики
            if (isUserNew) {
                ++newUsersCount;
            }
            else {
                ++notNewUsersCount;
            }
        }

        // Удаляем пустые категории и заполняем баджи с количеством пользователей
        if (newUsersCount > 0) {
            $('#badge-users-new-true').text(newUsersCount);
        } else {
            $('#users-header-new-true').remove();
            $('#users-new-true').remove();
        }
        if (notNewUsersCount > 0) {
            $('#badge-users-new-false').text(notNewUsersCount);
        } else {
            $('#users-header-new-false').remove();
            $('#users-new-false').remove();
        }

        finish_loading();

        let recordsCount = result.length;
        if (recordsCount > 0) {
            $.hulla.send(oneFewMany(recordsCount, "Загружен", "Загружено", "Загружено") + " " + recordsCount + " " + oneFewMany(recordsCount, "пользователь", "пользователя", "пользователей"), "success");
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