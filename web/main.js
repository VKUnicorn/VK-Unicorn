function start_loading() {
    $('#loading').show()
}

function finish_loading() {
    $('#loading').hide()
}

function main() {
    // Создаём и настраиваем плагин уведомлений
    $.hulla = new hullabaloo();
    $.hulla.options.delay = 2000;
    $.hulla.options.width = 250;
    $.hulla.options.offset.from = "bottom";
    $.hulla.options.allow_dismiss = false;
    $.hulla.options.alertClass = "settings-form-font";

    $.hulla.send('Загружено 10 профилей', "success");

    start_loading();
}