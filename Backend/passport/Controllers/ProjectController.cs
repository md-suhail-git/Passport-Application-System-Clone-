using passport.Helpers;
using passport.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace passport.Controllers
{
    //[Authorize]
    [RoutePrefix("api/project")]
    public class ProjectController : ApiController
    {
        private PassportEntities db=new PassportEntities();
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(RegisterDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Hash password
                byte[] passwordHash = System.Text.Encoding.UTF8.GetBytes(model.Password);

                string token = JwtTokenHelper.GenerateToken(
                    userId: 0,
                    fullName: model.FullName,
                    role: "Citizen",
                    loginID: model.LoginId
                );

                using (SqlConnection con = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_RegisterUser_WithJWT", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@FullName", model.FullName);
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@LoginID", model.LoginId);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@Nationality", model.Nationality);
                        cmd.Parameters.AddWithValue("@JwtToken", token);
                        cmd.Parameters.AddWithValue("@TokenExpiryMinutes", 60);

                        con.Open();

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            return Ok(new
                            {
                                Status = reader["Status"],
                                Message = reader["Message"],
                                UserId = reader["UserId"],
                                Token = token
                            });
                        }
                    }
                }

                return BadRequest("Registration failed.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
