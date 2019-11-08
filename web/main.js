function clear_workspace() {
    // Очищаем все элементы из основного рабочего контейнера
    $('#workspace').empty()

    // Скрываем кнопку добавления нового сообщества
    $('#add-group-button').hide()
}

function start_loading() {
    $('#loading').show()
}

function finish_loading() {
    $('#loading').fadeOut()
}

function main() {
    // Добавляем поддержку русского языка в bootbox
    var locale = {
        OK: 'Ок',
        CONFIRM: 'Продолжить',
        CANCEL: 'Отмена'
    };

    bootbox.addLocale('russian', locale);
    bootbox.setLocale('russian');

    // Создаём и настраиваем плагин уведомлений
    $.hulla = new hullabaloo();
    $.hulla.options.width = 270;
    $.hulla.options.offset.amount = 70;
    $.hulla.options.allow_dismiss = false;
    $.hulla.options.alertClass = "settings-form-font";

    // Тултипы на основные элементы интерфейса
    $('#display-groups').popover({
        trigger: 'hover',
        placement: 'left',
        delay: { "show": 450, "hide": 100 },
        content: 'Настроить сообщества'
    });

    $('#head-logo').popover({
        trigger: 'hover',
        placement: 'right',
        delay: { "show": 600, "hide": 100 },
        content: 'Привет!'
    });

    $('#add-group-button').popover({
        container: '#add-group-button',
        trigger: 'hover',
        placement: 'top',
        delay: { "show": 450, "hide": 100 },
        content: '<center>Добавить сообщество</center>'
    });

    // Загружаем пользователей как действие по умолчанию
    loadUsers();
}