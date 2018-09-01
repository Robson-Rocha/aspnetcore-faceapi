using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace RobsonAraujo.Demos.ASPNetCoreFaceAPI
{
    public class FaceServiceOptions
    {
        public string SubscriptionKey { get; set; }
        public string ApiRoot { get; set; }
    }

    public interface IFaceService
    {
        Task InitializePersonGroupAsync(string personGroupId, string personGroupName, IEnumerable<User> users);

        Task<User> IdentifyPersonAsync(string personGroupId, double confidenceThreshold, byte[] imageData, IEnumerable<User> users);
    }

    public class FaceService : IFaceService
    {
        private FaceServiceOptions Options {get; set;} 
        private FaceServiceClient Client {get; set;} 

        public FaceService(IOptions<FaceServiceOptions> options)
        {
            Options = options.Value;
            Client = new FaceServiceClient(Options.SubscriptionKey, Options.ApiRoot);
        }

        public async Task InitializePersonGroupAsync(string personGroupId, string personGroupName, IEnumerable<User> users)
        {
            //Check if there is an existing person group (and delete it for recreation)
            PersonGroup personGroup = null;
            try
            {
                personGroup = await Client.GetPersonGroupAsync(personGroupId);
                await Client.DeletePersonGroupAsync(personGroupId);
            }
            catch
            { }

            //Create the person group
            await Client.CreatePersonGroupAsync(personGroupId, personGroupName);

            //Fill the person group
            foreach (User user in users)
            {
                //Create the person
                CreatePersonResult createdPerson = await Client.CreatePersonAsync(personGroupId, user.Name);

                //Update the PersonId for the created person
                user.PersonId = createdPerson.PersonId;
                
                //Store the person face
                using (Stream imageStream = new MemoryStream(user.ImageBytes))
                    await Client.AddPersonFaceAsync(personGroupId, user.PersonId, imageStream);
            }

            //Train the Person Group
            await Client.TrainPersonGroupAsync(personGroupId);
            while (true)
            {
                //Check if the training is finished
                TrainingStatus trainingStatus = await Client.GetPersonGroupTrainingStatusAsync(personGroupId);
                if (trainingStatus.Status != Status.Running)
                    break;
                await Task.Delay(1000);
            }
        }

        public async Task<User> IdentifyPersonAsync(string personGroupId, double confidenceThreshold, byte[] imageData, IEnumerable<User> users)
        {
            File.WriteAllBytes(@"c:\_src\teste.png", imageData);
            //Detect the faces in the provided image
            Face[] faces = await Client.DetectAsync(new MemoryStream(imageData), returnFaceId: true);

            //Ensure that only one face must be present
            if(faces.Length != 1)
                throw new Exception("At least one, and only one face must be present at the image");

            Guid[] faceIds = faces.Select(f => f.FaceId).ToArray();

            //Identifying the detected faces
            User matchedUser = null;
            IdentifyResult[] results = await Client.IdentifyAsync(personGroupId, faceIds);
            foreach (IdentifyResult result in results)
            {
                //Get the best returned candidate
                Candidate bestCandidate = result.Candidates
                                                .Where(c => c.Confidence >= confidenceThreshold)
                                                .OrderByDescending(c => c.Confidence)
                                                .FirstOrDefault();

                //Get the person represented by the candidate
                Guid personId = bestCandidate.PersonId;
                // Person person = await Client.GetPersonAsync(personGroupId, personId);

                //Match the user corresponding to the identified person
                matchedUser = users.FirstOrDefault(u => u.PersonId == personId);
            }
            return matchedUser;
        }        
    }
}