using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using ZuoraMagic.Configuration;
using ZuoraMagic.Entities;
using ZuoraMagic.ORM.Models;
using ZuoraMagic.SoapApi.Responses;

namespace ZuoraMagic.Abstract
{
    public interface IZuoraClient : IDisposable
    {
        #region Public Fields

        ZuoraSession Session { get; }

        #endregion

        #region Session Methods

        /// <summary>
        ///     Login Action
        ///     - Stores session data for re-use
        /// </summary>
        /// <returns></returns>
        ZuoraSession Login();

        #endregion

        #region Query Methods

        /// <summary>
        ///     Generic predicate query method.
        ///     - Defaults to a limit of 2000. If a higher
        ///     limit is specified, ZuoraMagic will implement the
        ///     'queryMore' SOAPAction to capture all results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(int limit = 0) where T : ZObject;

        /// <summary>
        ///     Generic predicate query method.
        ///     - Defaults to a limit of 2000. If a higher
        ///     limit is specified, ZuoraMagic will implement the
        ///     'queryMore' SOAPAction to capture all results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : ZObject;

        /// <summary>
        ///     Generic string query method
        ///     - Defaults to a limit of 2000. If a higher
        ///     limit is specified, ZuoraMagic will implement the
        ///     'queryMore' SOAPAction to capture all results.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string query, int limit = 0) where T : ZObject;

        /// <summary>
        ///     Generic predicate query method.
        ///     Returns one record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        T QuerySingle<T>(Expression<Func<T, bool>> predicate) where T : ZObject;

        /// <summary>
        ///     Generic string query method.
        ///     Returns one record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        T QuerySingle<T>(string query) where T : ZObject;

        /// <summary>
        ///     Generic string query method implementing
        ///     deeper Zuora API features.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        QueryResult<T> PerformQuery<T>(string query, int limit = 0) where T : ZObject;

        /// <summary>
        ///     Generic predicate query method implementing
        ///     deeper Zuora API features.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        QueryResult<T> PerformQuery<T>(Expression<Func<T, bool>> predicate, int limit = 0) where T : ZObject;

        /// <summary>
        ///     Generic query more method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryLocator"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        QueryResult<T> PerformQueryMore<T>(string queryLocator, int limit = 0) where T : ZObject;

        #endregion

        #region Crud Methods

        ZuoraResponse Crud<T>(CrudOperation<T> operation) where T : ZObject;

        ZuoraResponse Insert<T>(IEnumerable<T> items) where T : ZObject;

        ZuoraResponse Insert<T>(T item) where T : ZObject;

        ZuoraResponse Update<T>(IEnumerable<T> items) where T : ZObject;

        ZuoraResponse Update<T>(T item) where T : ZObject;

        ZuoraResponse Delete<T>(IEnumerable<T> items) where T : ZObject;

        ZuoraResponse Delete<T>(T item) where T : ZObject;

        #endregion

        #region Export Methods

        IEnumerable<CsvRow> ExportData<T>(Expression<Func<T, bool>> predicate, ZuoraExportOptions options = null)
            where T : ZObject;

        IEnumerable<CsvRow> ExportData(string query, ZuoraExportOptions options = null);

        Stream ExportStream<T>(Expression<Func<T, bool>> predicate, ZuoraExportOptions options = null) where T : ZObject;

        Stream ExportStream(string query, ZuoraExportOptions options = null);

        IEnumerable<T> ExportRecords<T>(Expression<Func<T, bool>> predicate, ZuoraExportOptions options = null)
            where T : ZObject;

        IEnumerable<T> ExportRecords<T>(string query, ZuoraExportOptions options = null) where T : ZObject;

        ExportResult CreateExport(string query);

        ExportResult CreateExport<T>(Expression<Func<T, bool>> predicate, ZuoraExportOptions options = null) where T : ZObject;

        ExportResult CheckExportStatus(string id);

        Stream RetrieveExportStream(string id);

        IEnumerable<CsvRow> RetrieveExportData(string id);

        IEnumerable<T> RetrieveExportRecords<T>(string id, bool retrieveRelated = true) where T : ZObject;

        #endregion
    }
}