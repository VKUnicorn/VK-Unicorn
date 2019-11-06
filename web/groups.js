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
                trigger: 'hover',
                placement: 'bottom',
                delay: { "show": 200, "hide": 100 },
                content: 'Удалить группу навсегда'
            });

            groupCard.find('span.small-info-box').popover({
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

        let recordsCount = result.length;
        $.hulla.send(one_few_many(recordsCount, "Загружена", "Загружено", "Загружено") + " " + recordsCount + " " + one_few_many(recordsCount, "группа", "группы", "групп"), "success");

        // Показываем кнопку добавления новой группы если всё нормально загрузилось
        $('#add-group-button').show()
    }).fail(function(result) {
        finish_loading();

        let responseJSON = result['responseJSON'];
        if (responseJSON === undefined) {
            $.hulla.send("Ошибка при загрузке списка групп.<br>Главный модуль программы не запущен или в нём произошла внутренняя ошибка", "danger");
        }
        else {
            $.hulla.send(responseJSON.error, "danger");
        }
    })
}

function showAddGroupDialog() {
    bootbox.prompt({
        title: "Добавить группу",
        message: "Введите адрес группы или её короткое имя",
        placeholder: "https://vk.com/club123456",
        backdrop: true,
        callback: function (groupName) {
            if (groupName) {
                // Отправляем запрос на добавление новой группы
                $.post("add_group",
                {
                    url: groupName
                },
                function(data, status) {
                    if (status) {
                        $.hulla.send("Группа \"" + groupName + "\" добавлена<br>Она появится в списке после начальной обработки", "success");
                    }
                    else {
                        $.hulla.send("Не удалось добавить группу \"" + groupName + "\"", "danger");
                    }
                }).fail(function(result) {
                    $.hulla.send("Не удалось добавить группу \"" + groupName + "\"", "danger");
                });
            }
        }
    });
}