using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmscMeitrackProtocol.Core.Model
{
    public class SmscInputPacket
    {
        public int id { get; set; } //	Уникальный идентификатор входящего сообщения, назначаемый Сервером автоматически.
        public int sms_id { get; set; } //	Идентификатор сообщения, на которое получен ответ. Данный параметр отсутствует, если сообщение пришло не в качестве ответа (такие сообщения возможны при указании префикса "логин, двоеточие и пробел" либо при использовании выделенного входящего номера).
        public string phone { get; set; } //	Номер телефона абонента.
        public string mes { get; set; } //	Текст SMS-сообщения.
        public string to { get; set; } //	Входящий номер телефона, на который было отправлено сообщение абонентом.
        public DateTime? sent { get; set; } //	Время отправки сообщения абонентом в виде штампа в секундах.
        public DateTime? time { get; set; } //	Время получения сообщения Сервером в виде штампа в секундах. 
    }
}
