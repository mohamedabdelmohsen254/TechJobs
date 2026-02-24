import { useState, useEffect } from 'react';
import { filtersApi, syncApi } from '../services/api';
import { BlockedCompany, BlockedKeyword, FetchOptions } from '../types/job';

export function Filters() {
  const [blockedCompanies, setBlockedCompanies] = useState<BlockedCompany[]>([]);
  const [blockedKeywords, setBlockedKeywords] = useState<BlockedKeyword[]>([]);
  const [companySuggestions, setCompanySuggestions] = useState<string[]>([]);
  const [newCompany, setNewCompany] = useState('');
  const [newCompanyReason, setNewCompanyReason] = useState('');
  const [newKeyword, setNewKeyword] = useState('');
  const [newKeywordReason, setNewKeywordReason] = useState('');
  const [companySearch, setCompanySearch] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'companies' | 'keywords' | 'fetch'>('companies');
  
  // Fetch state
  const [fetching, setFetching] = useState(false);
  const [fetchResult, setFetchResult] = useState<any | null>(null);
  const [fetchOptions, setFetchOptions] = useState<FetchOptions>({
    fetchGreenhouse: true,
    fetchLever: true,
    fetchWorkable: true,
    fetchJooble: true,
    fetchRemoteOk: true,
    fetchRemotive: true,
    fetchHimalayas: true,
    fetchJobicy: true,
    joobleMaxPages: 3,
  });

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (companySearch.length >= 2) {
      loadCompanySuggestions();
    } else {
      setCompanySuggestions([]);
    }
  }, [companySearch]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [companies, keywords] = await Promise.all([
        filtersApi.getBlockedCompanies(),
        filtersApi.getBlockedKeywords(),
      ]);
      setBlockedCompanies(companies);
      setBlockedKeywords(keywords);
    } catch (err: any) {
      setError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const loadCompanySuggestions = async () => {
    try {
      const suggestions = await filtersApi.getCompanySuggestions(companySearch);
      setCompanySuggestions(suggestions);
    } catch (err) {
      console.error('Failed to load suggestions:', err);
    }
  };

  const handleAddCompany = async () => {
    if (!newCompany.trim()) return;
    
    try {
      const company = await filtersApi.addBlockedCompany({
        companyName: newCompany.trim(),
        reason: newCompanyReason.trim() || undefined,
      });
      setBlockedCompanies([company, ...blockedCompanies]);
      setNewCompany('');
      setNewCompanyReason('');
      setCompanySearch('');
      setSuccess('Company blocked successfully');
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to block company');
      setTimeout(() => setError(null), 3000);
    }
  };

  const handleToggleCompany = async (id: number) => {
    try {
      const updated = await filtersApi.toggleBlockedCompany(id);
      setBlockedCompanies(blockedCompanies.map(c => c.id === id ? updated : c));
    } catch (err: any) {
      setError(err.message || 'Failed to toggle company');
      setTimeout(() => setError(null), 3000);
    }
  };

  const handleDeleteCompany = async (id: number) => {
    if (!confirm('Are you sure you want to delete this blocked company?')) return;
    
    try {
      await filtersApi.deleteBlockedCompany(id);
      setBlockedCompanies(blockedCompanies.filter(c => c.id !== id));
      setSuccess('Company unblocked successfully');
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Failed to delete company');
      setTimeout(() => setError(null), 3000);
    }
  };

  const handleAddKeyword = async () => {
    if (!newKeyword.trim()) return;
    
    try {
      const keyword = await filtersApi.addBlockedKeyword({
        keyword: newKeyword.trim(),
        reason: newKeywordReason.trim() || undefined,
      });
      setBlockedKeywords([keyword, ...blockedKeywords]);
      setNewKeyword('');
      setNewKeywordReason('');
      setSuccess('Keyword blocked successfully');
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to block keyword');
      setTimeout(() => setError(null), 3000);
    }
  };

  const handleToggleKeyword = async (id: number) => {
    try {
      const updated = await filtersApi.toggleBlockedKeyword(id);
      setBlockedKeywords(blockedKeywords.map(k => k.id === id ? updated : k));
    } catch (err: any) {
      setError(err.message || 'Failed to toggle keyword');
      setTimeout(() => setError(null), 3000);
    }
  };

  const handleDeleteKeyword = async (id: number) => {
    if (!confirm('Are you sure you want to delete this blocked keyword?')) return;
    
    try {
      await filtersApi.deleteBlockedKeyword(id);
      setBlockedKeywords(blockedKeywords.filter(k => k.id !== id));
      setSuccess('Keyword unblocked successfully');
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Failed to delete keyword');
      setTimeout(() => setError(null), 3000);
    }
  };

  const handleFetchAndSync = async () => {
    setFetching(true);
    setFetchResult(null);
    setError(null);
    
    try {
      const result = await syncApi.fetchAndSync('http://localhost:5200', fetchOptions);
      setFetchResult(result);
      setSuccess(`Fetched ${result.fetchResult.totalFetched} jobs, imported ${result.syncResult.imported} new jobs`);
      setTimeout(() => setSuccess(null), 5000);
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Fetch failed');
    } finally {
      setFetching(false);
    }
  };

  const selectSuggestion = (company: string) => {
    setNewCompany(company);
    setCompanySearch('');
    setCompanySuggestions([]);
  };

  return (
    <div className="container mx-auto px-4 py-6">
      <h1 className="text-2xl font-bold mb-6">Job Filters & Fetch</h1>

      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      {success && (
        <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
          {success}
        </div>
      )}

      {/* Tabs */}
      <div className="border-b border-gray-200 mb-6">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('companies')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'companies'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Blocked Companies ({blockedCompanies.filter(c => c.isActive).length})
          </button>
          <button
            onClick={() => setActiveTab('keywords')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'keywords'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Blocked Keywords ({blockedKeywords.filter(k => k.isActive).length})
          </button>
          <button
            onClick={() => setActiveTab('fetch')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'fetch'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Fetch Jobs
          </button>
        </nav>
      </div>

      {loading ? (
        <div className="text-center py-8">Loading...</div>
      ) : (
        <>
          {/* Blocked Companies Tab */}
          {activeTab === 'companies' && (
            <div>
              <div className="bg-white rounded-lg shadow p-6 mb-6">
                <h2 className="text-lg font-semibold mb-4">Add Blocked Company</h2>
                <p className="text-gray-600 text-sm mb-4">
                  Jobs from blocked companies will not appear in the public job listings.
                </p>
                <div className="flex flex-col gap-3">
                  <div className="relative">
                    <input
                      type="text"
                      placeholder="Search or enter company name..."
                      value={companySearch || newCompany}
                      onChange={(e) => {
                        setCompanySearch(e.target.value);
                        setNewCompany(e.target.value);
                      }}
                      className="w-full px-3 py-2 border rounded-md"
                    />
                    {companySuggestions.length > 0 && (
                      <div className="absolute z-10 w-full mt-1 bg-white border rounded-md shadow-lg max-h-60 overflow-y-auto">
                        {companySuggestions.map((company, idx) => (
                          <button
                            key={idx}
                            onClick={() => selectSuggestion(company)}
                            className="w-full px-3 py-2 text-left hover:bg-gray-100"
                          >
                            {company}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                  <input
                    type="text"
                    placeholder="Reason (optional)"
                    value={newCompanyReason}
                    onChange={(e) => setNewCompanyReason(e.target.value)}
                    className="w-full px-3 py-2 border rounded-md"
                  />
                  <button
                    onClick={handleAddCompany}
                    disabled={!newCompany.trim()}
                    className="bg-blue-500 text-white px-4 py-2 rounded-md hover:bg-blue-600 disabled:bg-gray-300"
                  >
                    Block Company
                  </button>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow">
                <table className="min-w-full">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Company</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Reason</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {blockedCompanies.map((company) => (
                      <tr key={company.id}>
                        <td className="px-6 py-4 whitespace-nowrap">{company.companyName}</td>
                        <td className="px-6 py-4 text-sm text-gray-500">{company.reason || '-'}</td>
                        <td className="px-6 py-4">
                          <span className={`px-2 py-1 text-xs rounded-full ${
                            company.isActive ? 'bg-red-100 text-red-800' : 'bg-gray-100 text-gray-800'
                          }`}>
                            {company.isActive ? 'Blocked' : 'Inactive'}
                          </span>
                        </td>
                        <td className="px-6 py-4 space-x-2">
                          <button
                            onClick={() => handleToggleCompany(company.id)}
                            className="text-blue-600 hover:text-blue-800 text-sm"
                          >
                            {company.isActive ? 'Deactivate' : 'Activate'}
                          </button>
                          <button
                            onClick={() => handleDeleteCompany(company.id)}
                            className="text-red-600 hover:text-red-800 text-sm"
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                    {blockedCompanies.length === 0 && (
                      <tr>
                        <td colSpan={4} className="px-6 py-8 text-center text-gray-500">
                          No blocked companies yet
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Blocked Keywords Tab */}
          {activeTab === 'keywords' && (
            <div>
              <div className="bg-white rounded-lg shadow p-6 mb-6">
                <h2 className="text-lg font-semibold mb-4">Add Blocked Keyword</h2>
                <p className="text-gray-600 text-sm mb-4">
                  Jobs with titles containing blocked keywords will not appear in the public job listings.
                  Examples: accounting, banking, finance, legal, etc.
                </p>
                <div className="flex flex-col gap-3">
                  <input
                    type="text"
                    placeholder="Keyword to block..."
                    value={newKeyword}
                    onChange={(e) => setNewKeyword(e.target.value)}
                    className="w-full px-3 py-2 border rounded-md"
                  />
                  <input
                    type="text"
                    placeholder="Reason (optional)"
                    value={newKeywordReason}
                    onChange={(e) => setNewKeywordReason(e.target.value)}
                    className="w-full px-3 py-2 border rounded-md"
                  />
                  <button
                    onClick={handleAddKeyword}
                    disabled={!newKeyword.trim()}
                    className="bg-blue-500 text-white px-4 py-2 rounded-md hover:bg-blue-600 disabled:bg-gray-300"
                  >
                    Block Keyword
                  </button>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow">
                <table className="min-w-full">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Keyword</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Reason</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {blockedKeywords.map((keyword) => (
                      <tr key={keyword.id}>
                        <td className="px-6 py-4 whitespace-nowrap font-medium">{keyword.keyword}</td>
                        <td className="px-6 py-4 text-sm text-gray-500">{keyword.reason || '-'}</td>
                        <td className="px-6 py-4">
                          <span className={`px-2 py-1 text-xs rounded-full ${
                            keyword.isActive ? 'bg-red-100 text-red-800' : 'bg-gray-100 text-gray-800'
                          }`}>
                            {keyword.isActive ? 'Blocked' : 'Inactive'}
                          </span>
                        </td>
                        <td className="px-6 py-4 space-x-2">
                          <button
                            onClick={() => handleToggleKeyword(keyword.id)}
                            className="text-blue-600 hover:text-blue-800 text-sm"
                          >
                            {keyword.isActive ? 'Deactivate' : 'Activate'}
                          </button>
                          <button
                            onClick={() => handleDeleteKeyword(keyword.id)}
                            className="text-red-600 hover:text-red-800 text-sm"
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                    {blockedKeywords.length === 0 && (
                      <tr>
                        <td colSpan={4} className="px-6 py-8 text-center text-gray-500">
                          No blocked keywords yet
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* Fetch Jobs Tab */}
          {activeTab === 'fetch' && (
            <div>
              <div className="bg-white rounded-lg shadow p-6 mb-6">
                <h2 className="text-lg font-semibold mb-4">Fetch Jobs from External Sources</h2>
                <p className="text-gray-600 text-sm mb-4">
                  Fetch new jobs from Greenhouse, Lever, Workable, Jooble, and other sources.
                  New jobs will be automatically added to the database and visible to users.
                </p>
                
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                  {[
                    { key: 'fetchGreenhouse', label: 'Greenhouse' },
                    { key: 'fetchLever', label: 'Lever' },
                    { key: 'fetchWorkable', label: 'Workable' },
                    { key: 'fetchJooble', label: 'Jooble' },
                    { key: 'fetchRemoteOk', label: 'RemoteOK' },
                    { key: 'fetchRemotive', label: 'Remotive' },
                    { key: 'fetchHimalayas', label: 'Himalayas' },
                    { key: 'fetchJobicy', label: 'Jobicy' },
                  ].map(({ key, label }) => (
                    <label key={key} className="flex items-center space-x-2">
                      <input
                        type="checkbox"
                        checked={fetchOptions[key as keyof FetchOptions] as boolean}
                        onChange={(e) => setFetchOptions({ ...fetchOptions, [key]: e.target.checked })}
                        className="rounded"
                      />
                      <span>{label}</span>
                    </label>
                  ))}
                </div>

                <div className="mb-6">
                  <label className="block text-sm font-medium mb-1">Jooble Max Pages</label>
                  <input
                    type="number"
                    min="1"
                    max="10"
                    value={fetchOptions.joobleMaxPages || 3}
                    onChange={(e) => setFetchOptions({ ...fetchOptions, joobleMaxPages: parseInt(e.target.value) || 3 })}
                    className="w-24 px-3 py-2 border rounded-md"
                  />
                </div>

                <button
                  onClick={handleFetchAndSync}
                  disabled={fetching}
                  className="bg-green-500 text-white px-6 py-3 rounded-md hover:bg-green-600 disabled:bg-gray-300 font-medium"
                >
                  {fetching ? 'Fetching Jobs...' : 'Fetch & Sync Jobs'}
                </button>

                {fetching && (
                  <div className="mt-4 text-gray-600">
                    <div className="flex items-center space-x-2">
                      <svg className="animate-spin h-5 w-5 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      <span>This may take a few minutes...</span>
                    </div>
                  </div>
                )}
              </div>

              {fetchResult && (
                <div className="bg-white rounded-lg shadow p-6">
                  <h3 className="text-lg font-semibold mb-4 text-green-600">Fetch Completed!</h3>
                  
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                    <div className="bg-blue-50 p-4 rounded-lg">
                      <div className="text-2xl font-bold text-blue-600">{fetchResult.fetchResult?.totalFetched || 0}</div>
                      <div className="text-sm text-gray-600">Total Fetched</div>
                    </div>
                    <div className="bg-purple-50 p-4 rounded-lg">
                      <div className="text-2xl font-bold text-purple-600">{fetchResult.fetchResult?.afterDeduplication || 0}</div>
                      <div className="text-sm text-gray-600">After Dedup</div>
                    </div>
                    <div className="bg-green-50 p-4 rounded-lg">
                      <div className="text-2xl font-bold text-green-600">{fetchResult.syncResult?.imported || 0}</div>
                      <div className="text-sm text-gray-600">New Jobs Added</div>
                    </div>
                    <div className="bg-gray-50 p-4 rounded-lg">
                      <div className="text-2xl font-bold text-gray-600">{fetchResult.syncResult?.skipped || 0}</div>
                      <div className="text-sm text-gray-600">Skipped (Duplicates)</div>
                    </div>
                  </div>

                  {fetchResult.fetchResult?.sourceStats && (
                    <div>
                      <h4 className="font-medium mb-2">Jobs by Source:</h4>
                      <div className="flex flex-wrap gap-2">
                        {Object.entries(fetchResult.fetchResult.sourceStats).map(([source, count]) => (
                          <span key={source} className="px-3 py-1 bg-gray-100 rounded-full text-sm">
                            {source}: {String(count)}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}

                  <div className="mt-4 text-sm text-gray-500">
                    Duration: {(fetchResult.fetchResult?.durationSeconds || 0).toFixed(1)} seconds
                  </div>
                </div>
              )}
            </div>
          )}
        </>
      )}
    </div>
  );
}
