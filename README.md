# VsratiyAnnouncer

*Всратый аннаунсер* - это пет-проект, который слушает за игровыми событиями в Dota 2 используя GSI, а затем озвучивает 
игровые события мемными фразами через дискорд бота. Цель сделать как можно больше кастомизации, чтобы можно было менять 
звуковые файлы, отключать и включать озвучки определенных событий и редактировать их шанс выпада.


Данный репозиторий содержит два (+2) проекта сразу:
В папке *Agent* содержится код для агента, который написан на Node.js + NW.js. Его цель - выгрузка информации из дота 2 
на основной сервер (также называемый *Overlord*).

В папке *MainServer* содержится монолит на C#, состоящий из 2х подпроектов: *Overlord*, *DiscordBotProject*. 
*OverLord* производит менеджмент агентов, а также хранит в себе конфигурацию, согласно которой нужно озвучивать ивенты.
Конфигурация сделана по принципу HotSwap. Это приложение Blazor, поэтому он также выступает в роли админской веб-морды
(позволяет менять конфигурацию удалённо). *DiscordBot* предоставляет функции для управления дискорд ботом, 
а сам занимается его конфигурацией и подключением.

## Важно

Для работы проекта необходимо раздобыть и поместить `libsodium.dll`, `opus.dll` и `ffmpeg.exe` в корень папки **Overlord**