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
    $('#head-logo').popover({
        trigger: 'hover',
        placement: 'right',
        delay: { "show": 600, "hide": 100 },
        content: 'Привет!'
    });

    $('#donate-button').popover({
        template: getPopoverTemplateWithClass("donate-button-popover"),
        trigger: 'hover',
        placement: 'bottom',
        html: true,
        delay: { "show": 300, "hide": 100 },
        content: '<center>Поддержать проект можно по этой ссылке.<br>Любая сумма будет способствовать дальнейшему развитию проекта.<br>Там же можно дописать сообщение, я его обязательно увижу и прочитаю!<br><h6 class="mb-0">Спасибо!</h6></center>'
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