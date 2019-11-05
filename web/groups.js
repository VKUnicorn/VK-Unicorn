function loadGroups() {
    clear_workspace();
    start_loading();

    $.getJSON('groups', {
    }, function (result) {
        for (let groupExtraInfo of result) {
            let group = groupExtraInfo.data;

            // Заполняем карточку группы
            let groupCard = $(`
                <div class="col-sm-3 px-1 py-1">
                    <div class="card">
                        <div class="card-img-overlay px-1 py-1">
                            <a href=# class="btn btn-danger float-right px-2 py-2 delete-group-button"><i class="lni-close"></i></a>
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
                    </div>
                </div>
            `).appendTo($('#workspace'));

            // Тултипы
            groupCard.find('.delete-group-button').popover({
                container: 'body',
                trigger: 'hover',
                placement: 'bottom',
                delay: { "show": 200, "hide": 100 },
                content: 'Удалить группу навсегда'
            });

            groupCard.find('span.small-info-box').popover({
                container: 'body',
                trigger: 'hover',
                placement: 'top',
                delay: { "show": 200, "hide": 100 },
                content: 'Сколько профилей было найдено из этой группы'
            });

            // Оработчики событий
            groupCard.find('.delete-group-button').click(function(){
                bootbox.confirm({
                    title: "Удалить?",
                    message: "Вы действительно хотите навсегда удалить группу \"" + group.Name + "\"?",
                    backdrop: true,
                    callback: function (result) {
                        if (result) {
                            // Отправляем запрос на удаление
                            $.post("delete_group",
                            {
                                id: group.Id
                            },
                            function(data, status) {
                                if (status) {
                                    // Удаляем группу из UI
                                    groupCard.fadeOut();
                                }
                                else {
                                    $.hulla.send("Не удалось удалить группу \"" + group.Name + "\"", "danger");
                                }
                            }).fail(function(result) {
                                $.hulla.send("Не удалось удалить группу \"" + group.Name + "\"", "danger");
                            });
                        }
                    }
                });
            });
        }

        finish_loading();

        var recordsCount = result.length;
        $.hulla.send(one_few_many(recordsCount, "Загружена", "Загружено", "Загружено") + " " + recordsCount + " " + one_few_many(recordsCount, "группа", "группы", "групп"), "success");
    }).fail(function(result) {
        finish_loading();

        var responseJSON = result['responseJSON'];
        if (responseJSON === undefined) {
            $.hulla.send("Ошибка при загрузке списка групп. Главный модуль программы не запущен или в нём произошла внутренняя ошибка", "danger");
        }
        else {
            $.hulla.send(responseJSON.error, "danger");
        }
    })
}