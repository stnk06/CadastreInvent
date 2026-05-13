namespace CadastreInvent.Shared.Application.Auth
{
    public static class Permissions
    {
        public const string AdminAccess = "Permissions.Admin.Access";
        public const string DataDelete = "Permissions.Data.Delete";
        public const string CreateSpatialUnit = "Permissions.SpatialUnit.Create";
        public const string UpdateBoundary = "Permissions.SpatialUnit.UpdateBoundary";
        public const string ManageBAUnits = "Permissions.BAUnit.Manage";
        public const string ManageParties = "Permissions.Party.Manage";
        public const string RegisterRRR = "Permissions.RRR.Register";
        public const string TerminateRRR = "Permissions.RRR.Terminate";
        public const string CreateValuationUnit = "Permissions.ValuationUnit.Create";
        public const string RegisterTransaction = "Permissions.Transaction.Register";
        public const string ManageAppeals = "Permissions.Appeal.Manage";
        public const string ManageFieldTasks = "Permissions.FieldTask.Manage";
        public const string ExecuteFieldTasks = "Permissions.FieldTask.Execute";
        public const string ViewGisMap = "Permissions.Gis.ViewMap";
    }

    public static class AppRoles
    {
        public const string Admin = "Администратор ИС";
        public const string Employee = "Сотрудник отдела";
        public const string Inspector = "Полевой инспектор";
    }
}