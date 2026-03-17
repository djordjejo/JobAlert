import { useState, useEffect } from 'react'
import './App.css'

function App() {

  const [jobs, setJobs] = useState([])
  const [applications, setApplications] = useState([])

  useEffect(() => {
    fetch('https://localhost:58642/api/jobs')
      .then(response => response.json())
      .then(data => setJobs(data))
      .catch(error => console.error('Error fetching jobs:', error))
  }, [])

  useEffect(() => {
    const fetchJooble = async () => {
      const response = await fetch('https://localhost:58642/api/jobs/jooble', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
      });
      if (!response.ok) {
        console.error('Failed to fetch Jooble jobs:', response.status);
        return;
      }
      const data = await response.json();
      const joobleJobs = data.jobs.map(job => ({
        id: job.id,
        title: job.title,
        company: job.company,
        location: job.location,
        siteName: 'Jooble',
        url: job.link
      }));
      setJobs(prev => [...prev, ...joobleJobs]);
    }
    fetchJooble();
  }, [])

  useEffect(() => {
    const fetchApplications = async () => {
      const response = await fetch('https://localhost:58642/api/Application/GetAllApplications', {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
      });
      if (!response.ok) {
        console.error('Failed to fetch applications:', response.status);
        return;
      }
      const data = await response.json();
      setApplications(data);
    }
    fetchApplications();
  }, [])

  function handleApplication(jobId) {
    fetch(`https://localhost:58642/api/Application/apply/${jobId}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    }).then(response => {
      if (!response.ok) {
        console.error('Failed to apply for job:', response.status);
        return;
      }
      setApplications(prev => [...prev, { jobId: jobId }]);
    });
  }

  function handleDelete(jobId) {
    fetch(`https://localhost:58642/api/jobs/remove/${jobId}`, {
      method: 'DELETE'
    }).then(response => {
      if (!response.ok) {
        console.error('Failed to delete job:', response.status);
        return;
      }
      setJobs(prev => prev.filter(job => job.id !== jobId));
    });
  }

  return (
    <div className='flex justify-center items-center min-h-screen w-full bg-gray-950 px-6'>
      <div className='w-full max-w-6xl'>
        <h1 className='text-3xl font-bold text-white mb-6'>Job Listings</h1>
        <div className='overflow-hidden rounded-2xl border border-gray-800 shadow-2xl'>
          <table className='w-full text-sm text-left'>
            <thead className='bg-gray-900 text-gray-400 uppercase text-xs tracking-wider'>
              <tr>
                <th className='px-6 py-4'>Status</th>
                <th className='px-6 py-4'>Title</th>
                <th className='px-6 py-4'>Company</th>
                <th className='px-6 py-4'>Location</th>
                <th className='px-6 py-4'>Date posted</th>
                <th className='px-6 py-4'>Site</th>
                <th className='px-6 py-4'>Apply</th>
                <th className='px-6 py-4'>Konkuriši</th>
                <th className='px-6 py-4'>Obriši</th>
              </tr>
            </thead>
            <tbody className='divide-y divide-gray-800'>
              {jobs.map(job => (
                <tr
                  key={job.id}
                  className='bg-gray-900 hover:bg-gray-800 transition-colors duration-150'
                >
                  <td className='px-6 py-4'>
                    {applications.some(a => a.jobId === job.id)
                      ? <span className='text-yellow-400 text-xl' title='Konkurisao si!'>●</span>
                      : <span className='text-gray-700 text-xl'>●</span>
                    }
                  </td>
                  <td className='px-6 py-4 font-medium text-white'>{job.title}</td>
                  <td className='px-6 py-4 text-gray-300'>{job.company}</td>
                  <td className='px-6 py-4 text-gray-400'>{job.location}</td>
                  <td className='px-6 py-4 text-gray-400'>{job.dateposted}</td>
                  <td className='px-6 py-4 text-gray-400'>{job.siteName}</td>
                  <td className='px-6 py-4'>
                    <a
                    
                      href={job.url}
                      target='_blank'
                      rel='noopener noreferrer'
                      className='inline-block bg-blue-600 hover:bg-blue-500 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-colors duration-150'
                    >
                      Apply →
                    </a>
                  </td>
                  <td className='px-6 py-4'>
                    <button
                      onClick={() => handleApplication(job.id)}
                      disabled={applications.some(a => a.jobId === job.id)}
                      className={`ml-2 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-colors duration-150 ${applications.some(a => a.jobId === job.id) ? 'bg-gray-600 cursor-not-allowed' : 'bg-amber-600 hover:bg-amber-500 cursor-pointer'}`}
                    >
                      {applications.some(a => a.jobId === job.id) ? 'Konkurisao ✓' : 'Konkurisao sam!'}
                    </button>
                  </td>
                  <td className='px-6 py-4'>
                    <button
                      onClick={() => handleDelete(job.id)}
                      className='ml-2 bg-red-600 hover:bg-red-500 text-white text-xs font-semibold px-4 py-2 rounded-lg transition-colors duration-150'
                    >
                      Obriši!
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default App