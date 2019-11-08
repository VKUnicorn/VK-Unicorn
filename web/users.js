function loadUsers() {
    clear_workspace();
    start_loading();

    $.getJSON('users', {
    }, function(result) {
        for (let userExtraInfo of result) {
            let user = userExtraInfo.data;

            // Заполняем карточку пользователя
            let userCard = $(`
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