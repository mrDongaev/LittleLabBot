using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace littlelab_bot.BagWithAnimations
{
    public class StaticPhrase
    {
        public static readonly string[] _arrMaterials = new string[] { "Медь", "Сохранить и вернуться в меню" };

        public static readonly string[] _arrTests = new string[] { "Bi", "Se", "Ni", "Te", "Zn", "Si", "Sn", "Co", "Сохранить и вернуться в меню" };

        public const string _hello1 = "Проверим, есть ли вы в LittleLab!";

        public const string _choosematerial = "Выберите материал:";

        public const string _enterYourApplicationDetails = "Вносите данные по заявке";

        public const string _roleQuestion = "Вы не записаны в базе данных, кто вы, заказчик или химик?";

        public const string _startFillingOutApp = "Вы можете начать заполнять заявку в лабораторию!";

        public const string _expectApplications = "Ожидайте поступающих заявок!Для проверки активных заявок воспользуйтесь командой /checkactiverequest";

        public const string _enterId = "Отправьте идентификатор пробы в этот чат, далее, для подтверждения отправьте команду /finishenteringid, последнее отправленное сообщение будет считано как id";

        public const string _idAdded = "Id добавлен в шаблон заявки.";

        public const string _addedToTemplate = "Добавлено в шаблон заявки.";

        public const string _noRequestsToFill = "Нет наполняемых заявок, используйте команду /makerequest";

        public const string _selectIndicators = "Выберите необходимые показатели:";

        public const string _saveRequest = "Заявка сохранена";

        public const string _sentToTheLaboratory = "Заявка отправлена в лабораторию";

        public const string _acceptRequest = "Принять заявку";

        public const string _createSample = "Создать пробу";

        public const string _errorForSample = "Упс!Заявки больше нет.Обратитесь к администратору";

        public const string _errorForOutputSample = "Бот аварийно завершил работу, данных по пробе больше нет, заявку необходимо пересоздать!";

        public const string _enterAndChangeTheValue = "Вводите и изменяйте значение";

        public const string _sampleAndApplicationAreClosed = "--------------------------------- \n Образец и заявка закрыты.Данные направлены заказчику \n";

        public const string _notRequired = "Не требуется";

        public const string _client = "client";

        public const string _labworker = "labworker";

    }
}
