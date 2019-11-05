function loadProfiles() {
    clear_workspace();
    start_loading();

    $.getJSON('profiles', {
    }, function (result) {
        for (let profileExtraInfo of result) {
            let profile = profileExtraInfo.data;

            // Заполняем карточку профиля
            let profileCard = $(`

            `).appendTo($('#workspace'));
        }

        finish_loading();

        var recordsCount = result.length;
        if (recordsCount > 0) {
            $.hulla.send(one_few_many(recordsCount, "Загружен", "Загружено", "Загружено") + " " + recordsCount + " " + one_few_many(recordsCount, "профиль", "профиля", "профилей"), "success");
        }
        else {
            $.hulla.send("Не было загружено ни одного профиля, поэтому было открыто окно настройки групп", "success");

            loadGroups();
        }
    }).fail(function(result) {
        finish_loading();

        var responseJSON = result['responseJSON'];
        if (responseJSON === undefined) {
            $.hulla.send("Ошибка при загрузке списка профилей. Главный модуль программы не запущен или в нём произошла внутренняя ошибка", "danger");
        }
        else {
            $.hulla.send(responseJSON.error, "danger");
        }
    })
}