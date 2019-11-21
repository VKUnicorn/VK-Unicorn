function clear_workspace() {
    // Очищаем все элементы из основного рабочего контейнера
    $('#workspace').empty();

    // Скрываем кнопку добавления нового сообщества
    $('#add-group-button').hide();
}

function start_loading() {
    $('#loading').show();
}

function finish_loading() {
    $('#loading').fadeOut();
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

    let loadUsersButton = $('#load-users-button');
    loadUsersButton.popover({
        template: getPopoverTemplateWithClass("no-weight-limit"),
        trigger: 'hover',
        placement: 'bottom',
        html: true,
        delay: { "show": 550, "hide": 100 },
        content: 'По умолчанию загружаются только те пользователи, которые что-то<br>написали или поставили лайк за последние 60 дней<br><small class="text-muted">Shift-Click - загрузить всех сохранённых пользователей без лимита по времени</small>'
    });

    // Оработчики событий
    loadUsersButton.click(function(e) {
        // Если зажат shift, то загружаем вообще всех пользователей, без лимита по времени
        loadUsers(e);
    });

    $('#load-favorite-users-button').click(function(e) {
        loadUsers(e, true);
    });

    // Загружаем пользователей как действие по умолчанию
    loadUsers();
}