using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Service : System.Web.Services.WebService
{
    public Service () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    public class CheckIn
    {
        public string invitationID { get; set; }
        public string first_nm { get; set; }
        public string last_nm { get; set; }
        public string email { get; set; }
        public string eventName { get; set; }
        public string message { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string mobile_no { get; set; }
        public int intStatus { get; set; }
    }

    [WebMethod]
    public CheckIn CheckInAttendance(CheckIn invitationDetails)
    {
        string invitation_id = invitationDetails.invitationID;
        if (IsGuid(invitation_id))
        {
            // Connect database 
            string connString = ConfigurationManager.ConnectionStrings["ProductionDB"].ConnectionString;
            DataSet ds = new DataSet();
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                var command = new SqlCommand("select a.*, b.name from tbl_invitation a left join tbl_event b on a.event_id = b.id WHERE a.ID = '" + invitation_id + "'", conn);
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);
                conn.Close();
            }
            bool? invitationFound = null;
            bool? isAttend = null;
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                DataRow dr = ds.Tables[0].Rows[0];
                invitationFound = true;
                isAttend = dt.Columns.Contains("is_attend") ? Convert.ToBoolean(dr["is_attend"]) : true;
                invitationDetails.first_nm = dr["first_nm"] != DBNull.Value ? dr["first_nm"].ToString() : string.Empty;
                invitationDetails.last_nm = dr["last_nm"] != DBNull.Value ? dr["last_nm"].ToString() : string.Empty;
                invitationDetails.address = dr["address"] != DBNull.Value ? dr["address"].ToString() : string.Empty;
                invitationDetails.city = dr["city"] != DBNull.Value ? dr["city"].ToString() : string.Empty;
                invitationDetails.email = dr["email"] != DBNull.Value ? dr["email"].ToString() : string.Empty;
                invitationDetails.mobile_no = dr["mobile_no"] != DBNull.Value ? dr["mobile_no"].ToString() : string.Empty;
                invitationDetails.eventName = dr["name"] != DBNull.Value ? dr["name"].ToString() : string.Empty;
                //invitationDetails.first_nm = dr["city"] != DBNull.Value ? dr["city"].ToString() : string.Empty;
            }

            if (invitationFound.HasValue && !invitationFound.Value)
            {
                invitationDetails.intStatus = 1;
                invitationDetails.message = "No such invitation id";
                return invitationDetails;
            }
            else if (isAttend.HasValue && isAttend.Value)
            {
                invitationDetails.intStatus = 2;
                invitationDetails.message = "Already checked in";
                return invitationDetails;
            }
            else if (isAttend.HasValue && !isAttend.Value)
            {
                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();
                    var command = new SqlCommand("update tbl_invitation set is_attend = 1 WHERE ID = '" + invitation_id + "'", conn);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            


                invitationDetails.intStatus = 0;
                invitationDetails.message = "Checked in successfully";
                return invitationDetails;
            }
            else
            {
                invitationDetails.intStatus = -1;
                invitationDetails.message = "Invitation is invalid";
                return invitationDetails;
            }
        }
        else
        {
            invitationDetails.intStatus = -1;
            invitationDetails.message = "Invitation is invalid";
            return invitationDetails;
        }
            
    }

    public static bool IsGuid(string possibleGuid)
    {
        try
        {
            Guid gid = new Guid(possibleGuid);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
}