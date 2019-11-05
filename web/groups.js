function displayGroups() {
    clear_workspace();
    start_loading();

    $.getJSON('groups', {
    }, function (result) {
        for (let groupExtraInfo of result) {
            let group = groupExtraInfo.data;

            // Заполняем карточку группы
            let groupCard = $(`
                <div class="col-sm-3 px-1 py-1" id="group-holder" data-group-id="${group.Id}">
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

            groupCard.find('.delete-group-button').click(function(){
                bootbox.confirm({
                    title: "Удалить?",
                    message: "Вы действительно хотите навсегда удалить группу \"" + group.Name + "\"?",
                    locale: 'russian',
                    backdrop: true,
                    callback: function (result) {
                        if (result) {
                            // Отправляем запрос на удаление
                            $.post("delete-group",
                            {
                                id: groupCard.data("group-id")
                            },
                            function(data, status) {
                                if (status) {
                                    // Удаляем группу из UI
                                    groupCard.fadeOut();
                                }
                            });
                        }
                    }
                });
            });
        }


        /*
        for (let s of result['data']) {
            group_name = s.gr.name
            var is_closed_class = ''
            if (s.gr.is_closed == 1) {
                is_closed_class = 'close_group'
            }
            $('#cards').append(`<div class="col-sm-2 card-destroer" id="${s.gr.id}_card">
                    <div class="card card-inverse card-primary p-t-2">
                        <a href="https://vk.com/${s.gr.screen_name}" target="_blank"><img class="card-img-top" src="${s.gr.photo_200}"></a>
                        <div class="card-body p-1">
                            <h6 class="card-title font-weight-bold ${is_closed_class}" style="font-size: 9pt">
                               ${group_name}
                            </h6>
                            <ul class="list-group">
                              <li class="list-group-item p-1 font-weight-light" style="font-size: 9pt" title="Когда видели последнюю активность в группе">Активность: [${unixtime(s.active_ts)}]</small></li>
                              <li class="list-group-item p-1 font-weight-light" style="font-size: 9pt" title="Когда последний раз сканировали">Скан: [${unixtime(s.scan_ts)}]</small></li>
                              <li class="list-group-item p-1 font-weight-light" style="font-size: 9pt" title="Сколько времени занимает сканирование группы">Время: [${parseInt(s.scan_time)} сек.]</small></li>
                            </ul>

                            <p class="card-text " id="card_text_${s.gr.id}">
                                <li> <button id="group_delete_button_${s.gr.id}" type="button" class="btn btn-danger btn-sm mb-1" title="Для удаления  держать 5 секунд">Удалить</button></li>
                            </p>
                            <h6 class="card-subtitle mb-2 text-muted">
                            </h6>
                        </div>
                    </div>
                </div>
                `);
            $('#group_delete_button_' + s.gr.id).mayTriggerLongClicks().on('longClick', function (data) {
                settingsDeleteGroup(s.gr.id)
            });
        }
        */

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