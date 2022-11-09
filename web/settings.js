function isSettingsValid() {
    var isSettingsValid = false;

    $.ajax({
        url: 'is_settings_valid',
        dataType: 'json',
        async: false,
        success: function(result) {
            isSettingsValid = result[0]['Result'];
        }
    });

    return isSettingsValid;
}

function loadSettings(wasInvalidSettings) {
    // Загружаем настройки
    clear_workspace();
    start_loading();

    $.getJSON('settings', {
    }, function(result) {
        let settings = result[0]['Result'];
        let databaseFilename = result[0]['DatabaseFilename'];
        let wordsSeparator = result[0]['WordsSeparator'];

        function addCity(cityName, cityId) {
            return `<div class="col text-muted" id="cityHelper" data-city-help-id="${cityId}">${cityName} - ${cityId}</div>`;
        };

        $(`
            <div class="pr-2">
                <center>
                    <h2 class="mt-2 mb-0">
                        <font color="#ff6600">Добро пожаловать!</font>
                    </h2>
                </center>

                <h5 class="mb-1 mt-0"><i class="lni-cog"></i> ID приложения</h5>
                <hr class="mx-0 my-0">
                <div>
                    <p align="justify" class="mb-0">Для того чтобы можно было работать с API ВКонтакте нам нужен идентификатор приложения.
                    <br>API ВКонтакте позволяет нам отправлять специальные запросы, с помощью которых можно быстро сканировать сообщества и пользователей. К тому же это единственный оффициально поддерживаемый способ автоматизированно получать большое количество информации не опасаясь бана.
                    <br>Получить ID приложения совсем несложно.
                    Сначала логинимся на наш фейковый аккаунт ВКонтакте, который будет не жалко потерять в случае чего. На этом аккаунте должа быть отключена двухфакторная авторизация. Она пока не поддерживается так как нужно будет вводить СМС код подтверждения каждый раз.
                    <br>Залогинился на фейковый аккаунт? Далее делаем по пунктам:</p>
                    1. Открываем <a href="https://vk.com/editapp?act=create">эту ссылку</a> или вручную заходим в "Управление" - "Создать приложение"
                    <br>2. Устанавливаем настройки как тут и жмём "Подключить приложение". Название можно выбрать любое
                    <br><img src="settings_1.png" class="settings-img">
                    <br>3. Заходим в настройки и устанавливаем статус "Состояние" в "Приложение включено и видно всем"
                    <br><img src="settings_2.png" class="settings-img">
                    <br>4. Из поля выше копируем ID приложения и вставляем его в поле ввода ниже
                    <br><img src="settings_3.png" class="settings-img">
                    <br>
                    <div class="form-row">
                        <div class="col-md-4 mb-1">
                            <label for="cityId" class="mb-1"><b>ID приложения</b></label>
                            <input type="number" id="application-id" min="0" data-bind="value:replyNumber" value="${settings.ApplicationId}" style="width: 22.5rem" />
                        </div>
                    </div>
                </div>

                <h5 class="mb-1 mt-2"><i class="lni-vk rel-t-1"></i> Данные аккаунта</h5>
                <hr class="mx-0 my-0">
                <div>
                    <p align="justify" class="mb-0">Данные от фейкового аккаунта ВКонтакте. Этот же аккаунт будет добавляться в закрытые сообщества, поэтому выстави у него возраст 18+ и не делай закрытым. Нужен тот аккаунт, на котором добавляли приложение для получения его ID</p>
                    <div class="form-row">
                        <div class="col-md-4 mb-1">
                            <label for="login" class="mb-1"><b>Логин</b></label>
                            <input type="text" class="form-control" id="login" placeholder="это телефон или e-mail" value="${settings.Login}">
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="col-md-4 mb-1">
                            <label for="password" class="mb-1"><b>Пароль</b></label>
                            <input type="password" class="form-control" id="password" value="${settings.Password}">
                            <small class="form-text text-danger" align="justify">Логин и пароль будут сохранены в открытом виде в файле ${databaseFilename} в папке рядом с программой. Никому не высылай этот файл</small>
                        </div>
                    </div>
                </div>

                <h5 class="mb-1 mt-2"><i class="lni-map-marker rel-t-1"></i> ID города</h5>
                <hr class="mx-0 my-0">
                <div>
                    <p align="justify" class="mb-0">Это уникальный номер твоего города ВКонтакте. Его можно найти в таблице снизу или <a href="http://forum.botreg.ru/index.php?/topic/330-kak-uznat-id-goroda-vkontakte/" target="_blank">узнать тут</a>, если твоего города в этой таблице не оказалось. Кликни мышкой на твой город и он автоматически запишется в поле ввода.</p>
                    <small>
                    <div class="grid">
                        <div class="row mx-0">
                            ${addCity('Москва', 1)}
                            ${addCity('Новосибирск', 99)}
                            ${addCity('Челябинск', 158)}
                            ${addCity('Уфа', 151)}
                            ${addCity('Калининград', 61)}
                        </div>
                        <div class="row mx-0">
                            ${addCity('Санкт-Петербург', 2)}
                            ${addCity('Екатеринбург', 49)}
                            ${addCity('Омск', 104)}
                            ${addCity('Красноярск', 73)}
                            ${addCity('Краснодар', 72)}
                        </div>
                        <div class="row mx-0">
                            ${addCity('Киев', 314)}
                            ${addCity('Нижний Новгород', 95)}
                            ${addCity('Самара', 123)}
                            ${addCity('Пермь', 110)}
                            ${addCity('Владивосток', 37)}
                        </div>
                        <div class="row mx-0">
                            ${addCity('Минск', 282)}
                            ${addCity('Казань', 60)}
                            ${addCity('Ростов-на-Дону', 119)}
                            ${addCity('Волгоград', 10)}
                            ${addCity('Хабаровск', 153)}
                        </div>
                    </div>
                    </small>
                    <div class="form-row">
                        <div class="col-md-4 mb-1">
                            <label for="cityId" class="mb-1"><b>ID города</b></label>
                            <input type="number" id="city-id" min="1" data-bind="value:replyNumber" value="${settings.CityId}" style="width: 22.5rem" />
                        </div>
                    </div>
                </div>

                <h5 class="mb-1 mt-2"><i class="lni-target-audience rel-t-1"></i> Тип поиска</h5>
                <hr class="mx-0 my-0">
                <div>
                    <p align="justify" class="mb-0">По умолчанию отбираются пользователи, у которых указан город и он совпадает с твоим. Если сообщество закрытое, то из него берутся все пользователи. Ещё город не учитывается если пользователь сделал свой профиль закрытым.</p>
                    <div class="form-check">
                        <input class="form-check-input" type="radio" name="searchTypeRadios" id="searchTypeRadio0" value="0" ${settings.SearchMethod == 0 ? 'checked' : ''}>
                        <label class="form-check-label" for="searchTypeRadio0">
                            Все пользователи из закрытых сообществ, остальные по городу, включая закрытые профили <span class="text-success">(рекомендуется)</span>
                        </label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="radio" name="searchTypeRadios" id="searchTypeRadio1" value="1" ${settings.SearchMethod == 1 ? 'checked' : ''}>
                        <label class="form-check-label" for="searchTypeRadio1">
                            По городу. Только пользователи, у которых указан твой город, включая закрытые профили
                        </label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="radio" name="searchTypeRadios" id="searchTypeRadio2" value="2" ${settings.SearchMethod == 2 ? 'checked' : ''}>
                        <label class="form-check-label" for="searchTypeRadio2">
                            Все. Ищет всех пользователей женского пола.
                        </label>
                    </div>
                </div>

                <h5 class="mb-1 mt-2"><i class="lni-warning rel-t-1"></i> Стоп слова</h5>
                <hr class="mx-0 my-0">
                <div>
                    <p align="justify" class="mb-0">Если в записи будет найдено стоп слово, то пользователь будет отмечен специальной жёлтой рамкой, но всё равно будет виден в результатах поиска. Регистр стоп слов не учитывается. По умолчанию было добавлено несколько стоп слов для примера.</p>
                    <input name="tags-stop-words" id="stop-words" value="${settings.StopWords.split(wordsSeparator)}">
                </div>

                <h5 class="mb-1 mt-2"><i class="lni-trash rel-t-1"></i> Чёрный список</h5>
                <hr class="mx-0 my-0">
                <div>
                    <p align="justify" class="mb-0">Если в записи, комментарии или статусе пользователя будет найдено слово из чёрного списка, то эта запись, комментарий или пользователь не будут видны в результатах поиска. Какая-то девушка постоянно спамит своим номером телефона со взломанных страниц? Самое время добавить этот номер телефона в чёрный список. Регистр слов в чёрном списке не учитывается. Эти настройки не повлияют на уже добавленные записи, комментарии и пользователей. По умолчанию было добавлено несколько слов для примера.</p>
                    <input name="tags-blacklist-words" id="blacklist-words" value="${settings.BlacklistWords.split(wordsSeparator)}">
                </div>

                <h5 class="mb-1 mt-2"><i class="lni-image rel-t-1"></i> Посты-картинки</h5>
                <hr class="mx-0 my-0">
                <div>
                    <p align="justify" class="mb-0">Многие боты очень любят часто постить однотипные картинки, в которых содержатся только лишь реклама. Эта опция позволяет автоматически игнорировать подобные записи. Не повлияет на уже добавленные записи и пользователей.</p>
                    <input type="checkbox" id="ignoreOnlyImagePosts" ${settings.IgnoreOnlyImagePosts ? 'checked' : ''}>
                    <label class="form-check-label" for="ignoreOnlyImagePosts">
                        Игнорировать пустые записи, в которых содержится только одно изображение <span class="text-success">(рекомендуется)</span>
                    </label>
                </div>

                <div class="mt-3 mb-5">
                    <button type="button" class="btn btn-success btn-lg" id="save-settings-button">Сохранить все настройки</button>
                </div>
            </div>
        `).appendTo($('#workspace'));

        // Назначаем вспомогательные события нажатия на таблицу с городами
        $('*[id=cityHelper]').click(function() {
            $("#city-id").val($(this).data('city-help-id'));
        });

        $('#save-settings-button').click(function() {
            $.post("save_settings",
            {
                applicationId : $('#application-id').val(),
                login : $('#login').val(),
                password : $('#password').val(),
                cityId : $('#city-id').val(),
                searchType : $("input:radio:checked").val(),
                stopWords : $('#stop-words').val(),
                blacklistWords : $('#blacklist-words').val(),
                ignoreOnlyImagePosts : $('#ignoreOnlyImagePosts').is(':checked')
            },
            function(data, status) {
                if (status) {
                    $.hulla.send("Настройки успешно сохранены!", "success");

                    // Настройки были неправильными, а теперь могут быть правильными. Пробуем загрузить пользователей
                    if (wasInvalidSettings) {
                        loadUsers();
                    }
                }
                else {
                    $.hulla.send("Не удалось сохранить настройки", "danger");
                }
            }).fail(function(result) {
                $.hulla.send("Не удалось сохранить настройки", "danger");
            });
        });

        // Создаём и настраиваем плагин для стоп-слов
        new Tagify(document.querySelector('input[name=tags-stop-words]'));
        new Tagify(document.querySelector('input[name=tags-blacklist-words]'));

        finish_loading();
    }).fail(function(result) {
        finish_loading();

        $.hulla.send("Ошибка при загрузке настроек.<br>Главный модуль программы не запущен или в нём произошла внутренняя ошибка", "danger");
    });
}