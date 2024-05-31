
using System.Collections.Generic;
using System.ServiceModel;

namespace WcfServiceBook
{
    [ServiceContract]
    public interface IServiceBook
    {
        [OperationContract]
        int SignIn(string login, string password);

        [OperationContract]
        List<int> SearchUsers(int idIgnore, string searchText);

        [OperationContract]
        List<int> SearchBooks(int idIgnore, string searchText);

        [OperationContract]
        int EditUserData(int userId, string login, string password, string userInformation);

        [OperationContract]
        void DeleteAccount(int userId);

        [OperationContract]
        void DeleteBook(int bookId);

        [OperationContract]
        string GetName(int userId);

        [OperationContract]
        string[] GetBook(int bookId);

        [OperationContract]
        List<int> GetBooksId(int userId);

        [OperationContract]
        string GetDescription(int userId);

        [OperationContract]
        string GetPassword(int userId);

        [OperationContract]
        int RegisterUser(string login, string password);

        [OperationContract]
        int Put(FileTransferRequest[] filesToPush, string name, string description, int userId);

        [OperationContract]
        void Put1(FileTransferRequest fileToPush, int bookId, int userId, string path);

        [OperationContract]
        int EditBookName(int bookId, string name);

        [OperationContract]
        void EditBookDescription(int bookId, string description);

        [OperationContract]
        void EditBookCanEdit(int bookId, int canEdit);

        [OperationContract]
        void EditBookAccess(int bookId, int access);

        [OperationContract]
        FileTransferRequest[] Get(int bookId, int userId);

        [OperationContract]
        int GetUserId(int bookId);
    }
}
