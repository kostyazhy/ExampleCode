# LoaderBundles
Данный код является частью приложения Мегамозг 3D.
Код отвечает за формирование списка доступных asset bundles.
Первое, что он выполняет - это запрашивает доступные asset bundles в API игры.
После получения json с доступными бандлами, класс парсит информацию
в класс InfoBundle и формирует графическое представление списка.
У каждого элемента списка есть кнопка, которая запускает одно из 
двух действий:
- загрузить бандл на устройство;
- удалить бандл с устройства.   

После того как действие выполнилось класс изменяет внешний вид выбранного элемента списка.
